﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Linq;
using System.Threading.Tasks;

namespace CLIParserSourceGenerator
{
    internal class ProgramGenerator
    {
        private string _className { get; }
        private string _optionsClassName { get; }
        private string _parserClassName { get; }
        private string _helpText { get; }
        private MainReturnTypeEnum _mainReturnType { get; }
        private List<OptionInfo> _options { get; }

        public ProgramGenerator(List<OptionInfo> options, string optionsClassName, string parserClassName, string helpText, MainReturnTypeEnum mainReturnType)
        {
            this._options = options;
            this._className = "AutoGeneratedProgram";
            this._optionsClassName = optionsClassName;
            this._parserClassName = parserClassName;
            this._helpText = helpText;
            this._mainReturnType = mainReturnType;
        }

        public string GenerateSource()
        {
            return this.Generate().NormalizeWhitespace().ToFullString();
        }

        private ClassDeclarationSyntax Generate()
        {
            return ClassDeclaration(this._className)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.InternalKeyword)
                    )
                )
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        this.GenerateMethodStub()
                        .WithParameterList(
                            ParameterList(
                                SingletonSeparatedList<ParameterSyntax>(
                                    Parameter(
                                        Identifier("args")
                                    )
                                    .WithType(
                                        ArrayType(
                                            PredefinedType(
                                                Token(SyntaxKind.StringKeyword)
                                            )
                                        )
                                        .WithRankSpecifiers(
                                            SingletonList<ArrayRankSpecifierSyntax>(
                                                ArrayRankSpecifier(
                                                    SingletonSeparatedList<ExpressionSyntax>(
                                                        OmittedArraySizeExpression()
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                        .WithBody(
                            Block(
                                LocalDeclarationStatement(
                                    VariableDeclaration(
                                        PredefinedType(
                                            Token(SyntaxKind.StringKeyword)
                                        )
                                    )
                                    .WithVariables(
                                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                                            VariableDeclarator(
                                                Identifier("helpText")
                                            )
                                            .WithInitializer(
                                                EqualsValueClause(
                                                    LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        Token(SyntaxTriviaList.Empty, SyntaxKind.MultiLineRawStringLiteralToken, $"\"\"\"\n{this._helpText}\n\"\"\"", this._helpText, SyntaxTriviaList.Empty)
                                                    )
                                                )
                                            )
                                        )
                                    )
                                ),
                                ParseStatement($"{this._parserClassName} parser = new();"),
                                ParseStatement($"{this._optionsClassName} options = parser.Parse(args);"),
                                IfStatement(
                                    ParseExpression("options.___ShowHelp___"),
                                    Block(
                                        ParseStatement("Console.WriteLine(helpText);"),
                                        ParseStatement(this.GenerateHelpReturnStyle())
                                    )
                                ),
                                // TODO: the should return, await, or simply call the main method based on its return type
                                ParseStatement($"{this.GenerateReturnStyle()}Program.Main({string.Join(", ", this._options.Select(o => $"@{o.Parameter.ParameterName}: options.@{o.PropertyName}"))});")
                            )
                        )
                    )
                );
        }

        protected string GenerateHelpReturnStyle()
        {
            return this._mainReturnType switch
            {
                MainReturnTypeEnum.Void => "return;",
                MainReturnTypeEnum.Int => "return 0;",
                MainReturnTypeEnum.Task => "return;",
                MainReturnTypeEnum.TaskInt => "return 0;",
                _ => throw new ArgumentException($"Unexpected return type '{this._mainReturnType}'."),
            };
        }

        protected string GenerateReturnStyle()
        {
            return this._mainReturnType switch
            {
                MainReturnTypeEnum.Void => "",
                MainReturnTypeEnum.Int => "return ",
                MainReturnTypeEnum.Task => "await ",
                MainReturnTypeEnum.TaskInt => "return await ",
                _ => throw new ArgumentException($"Unexpected return type '{this._mainReturnType}'."),
            };
        }

        protected MethodDeclarationSyntax GenerateMethodStub()
        {
            return this._mainReturnType switch
            {
                MainReturnTypeEnum.Void => this.GenerateVoidMethodStub(),
                MainReturnTypeEnum.Int => this.GenerateIntMethodStub(),
                MainReturnTypeEnum.Task => this.GenerateTaskMethodStub(),
                MainReturnTypeEnum.TaskInt => this.GenerateTaskIntMethodStub(),
                _ => throw new ArgumentException($"Unexpected return type '{this._mainReturnType}'."),
            };
        }

        protected MethodDeclarationSyntax GenerateVoidMethodStub()
        {
            return MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.VoidKeyword)
                ),
                Identifier("Main")
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.StaticKeyword)
                )
            );
        }

        protected MethodDeclarationSyntax GenerateIntMethodStub()
        {
            return MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.IntKeyword)
                ),
                Identifier("Main")
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.StaticKeyword)
                )
            );
        }

        protected MethodDeclarationSyntax GenerateTaskMethodStub()
        {
            return MethodDeclaration(
                IdentifierName("Task"),
                Identifier("Main")
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.StaticKeyword),
                    Token(SyntaxKind.AsyncKeyword)
                )
            );
        }

        protected MethodDeclarationSyntax GenerateTaskIntMethodStub()
        {
            return MethodDeclaration(
                GenericName(
                    Identifier("Task")
                )
                .WithTypeArgumentList(
                    TypeArgumentList(
                        SingletonSeparatedList<TypeSyntax>(
                            PredefinedType(
                                Token(SyntaxKind.IntKeyword)
                            )
                        )
                    )
                ),
                Identifier("Main")
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.StaticKeyword),
                    Token(SyntaxKind.AsyncKeyword)
                )
            );
        }
    }
}
