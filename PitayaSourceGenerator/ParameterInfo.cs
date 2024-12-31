using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CLIParserSourceGenerator
{
    internal class ParameterInfo
    {
        public string ParameterName { get; }
        public ITypeSymbol Type { get;  }
        public bool IsNullable { get; }
        public bool HasDefaultValue { get; }
        public object? DefaultValue { get; set; }

        public ParameterInfo(string parameterName, ITypeSymbol type, bool hasDefaultValue, object? defaultValue = null)
        {
            this.ParameterName = parameterName;
            this.Type = type;
            this.IsNullable = type.NullableAnnotation == NullableAnnotation.Annotated;
            this.HasDefaultValue = hasDefaultValue;
            this.DefaultValue = defaultValue;
        }

        public static ParameterInfo Create(IParameterSymbol parameter)
        {
            return new ParameterInfo(
                parameterName: parameter.Name,
                type: parameter.Type,
                hasDefaultValue: parameter.HasExplicitDefaultValue || parameter.Type.Name == "bool",
                defaultValue: parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue : (parameter.Type.Name == "bool" ? "false" : null)
            );
        }
    }
}
