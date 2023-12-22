using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CLIParserSourceGenerator
{
    internal class OptionInfo
    {
        public string OptionName { get; }

        public string PropertyName { get; }

        public ParameterInfo Parameter { get; }

        public bool IsArrayLike { get; }

        public string? BackingListName { get; }

        public string? ValueSetPropertyName { get; }

        public ITypeSymbol? ElementType { get; }

        /// <summary>
        /// Hold info about a parameter that is an option
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="propertyName"></param>
        /// <param name="parameter"></param>
        /// <param name="isArrayLike"></param>
        /// <param name="backingListName"></param>
        /// <param name="valueSetPropertyName"></param>
        public OptionInfo(string optionName, string propertyName, ParameterInfo parameter, bool isArrayLike, string? backingListName, string? valueSetPropertyName, ITypeSymbol? elementType)
        {
            this.OptionName = optionName;
            this.PropertyName = propertyName;
            this.Parameter = parameter;
            this.IsArrayLike = isArrayLike;
            this.BackingListName = backingListName;
            this.ValueSetPropertyName = valueSetPropertyName;
            this.ElementType = elementType;
        }

        public static OptionInfo Create(ParameterInfo parameter)
        {
            string parameterType = parameter.Type.ToDisplayString();
            bool isArrayLike = parameterType.EndsWith("[]") || parameterType.StartsWith("System.Collections.Generic.List<");
            string propertyName = Utilities.Propertyify(parameter.ParameterName);
            return new OptionInfo(
                optionName: Utilities.Optionify(parameter.ParameterName)
                ,propertyName: propertyName
                ,parameter: parameter
                ,isArrayLike: isArrayLike
                ,backingListName: isArrayLike ? $"_{propertyName}BackingList" : null
                ,valueSetPropertyName: !(parameter.HasDefaultValue || parameter.IsNullable) ? $"_{propertyName}ValueSet" : null
                ,elementType: isArrayLike ? (parameter.Type as IArrayTypeSymbol)?.ElementType ?? (parameter.Type as INamedTypeSymbol)?.TypeArguments.FirstOrDefault() : null
            );
        }

        /// <summary>
        /// Generate properties required for the option class
        /// </summary>
        /// <returns></returns>
        public List<MemberDeclarationSyntax> GenerateOptionProperties()
        {
            List<MemberDeclarationSyntax> properties = new List<MemberDeclarationSyntax>();

            // if the parameter is an array, need to generate extra properties
            if (this.IsArrayLike)
            {
                properties.Add(this.BasicArrayProperty());
                properties.Add(this.ListBackingProperty());
            }
            else
            {
                properties.Add(this.BasicProperty());
            }

            if (this.ValueSetPropertyName != null)
            {
                properties.Add(this.ValueSetProperty());
            }

            return properties;
        }

        internal MemberDeclarationSyntax ValueSetProperty()
        {
            string property = $"public bool {this.ValueSetPropertyName} {{ get; set; }}";
            MemberDeclarationSyntax? propertySyntax = ParseMemberDeclaration(property);
            if (propertySyntax is null)
            {
                throw new Exception($"Failed generate HasValue property for option {this.OptionName}");
            }

            return propertySyntax;
        }

        internal MemberDeclarationSyntax ListBackingProperty()
        {
            // TODO: the type of this list should be the type of the array, not the type of the parameter
            string property = $"public List<{(this.ElementType)?.ToDisplayString()}> {this.BackingListName} = new();";
            MemberDeclarationSyntax? propertySyntax = ParseMemberDeclaration(property);
            if (propertySyntax is null)
            {
                throw new Exception($"Failed generate backing list property for option {this.OptionName}");
            }

            return propertySyntax;
        }

        internal MemberDeclarationSyntax BasicArrayProperty()
        {
            string property = $"public {this.Parameter.Type.ToDisplayString()} {this.PropertyName} {{ get {{ return this.{this.BackingListName}{this.GetArrayConversionMethod()}; }} }}";
            MemberDeclarationSyntax? propertySyntax = ParseMemberDeclaration(property);
            if (propertySyntax is null)
            {
                throw new Exception($"Failed generate array property for option {this.OptionName}");
            }

            return propertySyntax;
        }

        /// <summary>
        /// Take the type of the parameter and return the method to convert the backing list to the same enumerable type
        /// </summary>
        /// <returns></returns>
        internal string GetArrayConversionMethod()
        {
            string type = this.Parameter.Type.ToDisplayString();
            if (type.EndsWith("[]"))
            {
                return ".ToArray()";
            }
            else if (type.StartsWith("System.Collections.Generic.List<"))
            {
                return "";
            }
            else
            {
                throw new Exception($"Failed to determine array conversion method for option {this.OptionName}");
            }
        }

        internal MemberDeclarationSyntax BasicProperty()
        {
            string type = this.Parameter.Type.ToDisplayString();
            string nullable = this.Parameter.IsNullable ? "?" : "";
            string defaultValue = this.Parameter.HasDefaultValue ? $" = {this.GetDefaultValue()};" : "";
            string property = $"public {type}{nullable} {this.PropertyName} {{ get; set; }}{defaultValue}";
            MemberDeclarationSyntax? propertySyntax = ParseMemberDeclaration(property);
            if (propertySyntax is null)
            {
                throw new Exception($"Failed generate property for option {this.OptionName}");
            }

            return propertySyntax;
        }

        internal object GetDefaultValue()
        {
            if (this.Parameter.DefaultValue is null)
            {
                throw new Exception($"Failed to get default value for option {this.OptionName}");
            }

            if (this.Parameter.Type.ToDisplayString() == "string")
            {
                return $"\"{this.Parameter.DefaultValue}\"";
            }

            return this.Parameter.DefaultValue;
        }

        public SwitchSectionSyntax GenerateSwitchSection()
        {
            List<StatementSyntax> statements = new();
            if (this.IsArrayLike)
            {
                statements.Add(this.GenerateArrayLikeParseStatement());
            }
            else
            {
                statements.Add(GenerateParseStatement(this.PropertyName, this.Parameter.Type));
            }

            if (this.ValueSetPropertyName != null)
            {
                statements.Add(ParseStatement($"options.{this.ValueSetPropertyName} = true;"));
            }

            statements.Add(BreakStatement());

            return SwitchSection()
                .WithLabels(
                    SingletonList<SwitchLabelSyntax>(
                        CaseSwitchLabel(
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(this.OptionName)
                            )
                        )
                    )
                )
                .WithStatements(
                    List<StatementSyntax>(statements)
                );
        }

        private StatementSyntax GenerateArrayLikeParseStatement()
        {
            return ForStatement(
                Block(
                    ParseStatement("arg = args[i];"),
                    IfStatement(
                        ParseExpression("arg.StartsWith(\"-\") && (arg.StartsWith(\"--\") || arg == \"-h\" || arg == \"-?\")"),
                        Block(
                            ParseStatement("i--;"),
                            BreakStatement()
                        )
                    )
                    .WithElse(
                        ElseClause(
                            Block(
                                SingletonList<StatementSyntax>(
                                    GenerateArrayLikeParseStatement(this.BackingListName!, this.ElementType!)
                                )
                            )
                        )
                    )
                )
            )
            .WithCondition(
                ParseExpression("i < args.Length")
            )
            .WithIncrementors(
                SingletonSeparatedList<ExpressionSyntax>(ParseExpression("i++"))
            );
        }

        private static StatementSyntax GenerateParseStatement(string propertyName, ITypeSymbol parameter)
        {
            return ParseStatement($"options.{propertyName} = {GenerateParseExpression(parameter)};");
        }

        private static StatementSyntax GenerateArrayLikeParseStatement(string propertyName, ITypeSymbol parameter)
        {
            return ParseStatement($"options.{propertyName}.Add({GenerateParseExpression(parameter)});");
        }

        private static string GenerateParseExpression(ITypeSymbol parameter)
        {
            if (parameter.ToDisplayString() == "string")
            {
                return "args[i]";
            }
            else if (parameter.ToDisplayString() == "System.Uri")
            {
                return "new Uri(args[i])";
            }
            else if (parameter.IsValueType)
            {
                return $"{parameter.ToDisplayString()}.Parse(args[i])";
            }
            else
            {
                // check if the type has a Parse method
                // if it does, use that

                var possibleCreationMethods = parameter.GetMembers()
                    .Where(m => (m.Name == "Parse" || m.Name == ".ctor")
                        && m is IMethodSymbol ms
                        && ms.Parameters.Length >= 1
                        && ms.Parameters[0].Type.ToDisplayString() == "string"
                        && (ms.Parameters.Length == 1 || ms.Parameters.Skip(1).All(p => p.IsOptional))
                        && (m.Name == ".ctor" || ms.ReturnType.ToDisplayString() == parameter.ToDisplayString())
                        && (m.Name == ".ctor" || ms.IsStatic)
                        && !ms.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Obsolete"))
                    .ToList();

                if (possibleCreationMethods.Any(m => m.Name == "Parse"))
                {
                    return $"{parameter.ToDisplayString()}.Parse(args[i])";
                }
                else if (possibleCreationMethods.Any(m => m.Name == ".ctor"))
                {
                    return $"new {parameter.ToDisplayString()}(args[i])";
                }
                else
                {
                    throw new Exception($"Failed to find a way to parse {parameter.ToDisplayString()}");
                }
            }
        }

        public ExpressionSyntax? GenerateSetValueAccess()
        {
            return this.ValueSetPropertyName == null
                ? null
                : ParseExpression($"options.{this.ValueSetPropertyName}");
        }

        public StatementSyntax? GenerateSpecificMissingOptionCheck()
        {
            return this.ValueSetPropertyName == null
                ? null
                : IfStatement(
                    PrefixUnaryExpression(
                        SyntaxKind.LogicalNotExpression,
                        this.GenerateSetValueAccess()!  // this won't be null because of the null check above
                    ),
                    Block(
                        SingletonList<StatementSyntax>(
                            ParseStatement($"missing.Add(\"{this.OptionName}\");")
                        )
                    )
                );
        }
    }
}
