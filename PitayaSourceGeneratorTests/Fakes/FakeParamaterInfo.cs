using Microsoft.CodeAnalysis;
using Moq;
using CLIParserSourceGenerator;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Sockets;

namespace CLIParserSourceGeneratorTests.Fakes
{
    class FakeParameterInfo : ParameterInfo
    {
        private static IParameterSymbol? _stringParameter;
        private static IParameterSymbol StringParameter
        {
            get
            {
                if (_stringParameter is null)
                {
                    var mockedStringType = new Mock<ITypeSymbol>(MockBehavior.Strict);
                    mockedStringType.SetupGet(t => t.Name).Returns("System.String");
                    mockedStringType.Setup(t => t.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("string");

                    var mockedStringParameter = new Mock<IParameterSymbol>(MockBehavior.Strict);
                    mockedStringParameter.SetupGet(t => t.Name).Returns("aValue");
                    mockedStringParameter.SetupGet(t => t.Type).Returns(mockedStringType.Object);
                    _stringParameter = mockedStringParameter.Object;
                }

                return _stringParameter;
            }
        }

        private static IMethodSymbol? _constructor;
        public static IMethodSymbol Constructor
        {
            get
            {
                if (_constructor is null)
                {
                    var mockedConstructorMethod = new Mock<IMethodSymbol>(MockBehavior.Strict);
                    mockedConstructorMethod.SetupGet(t => t.Name).Returns(".ctor");
                    mockedConstructorMethod.SetupGet(t => t.Parameters).Returns(ImmutableArray.Create<IParameterSymbol>(StringParameter));
                    mockedConstructorMethod.Setup(t => t.GetAttributes()).Returns(ImmutableArray.Create<AttributeData>());
                    _constructor = mockedConstructorMethod.Object;
                }

                return _constructor;
            }
        }

        public FakeParameterInfo(string parameterName, ITypeSymbol type, bool hasDefaultValue, object? defaultValue = null) : base(parameterName, type, hasDefaultValue, defaultValue)
        {
        }

        public static FakeParameterInfo Create(
            string? parameterName = null,
            ITypeSymbol? type = null,
            bool? hasDefaultValue = null,
            object? defaultValue = null,
            NullableAnnotation? nullableAnnotation = null,
            string? typeName = null,
            List<IMethodSymbol>? members = null
        )
        {
            string paramName = parameterName ?? "aValue";

            if (type is null)
            {
                Mock<ITypeSymbol> mockedType = MockType(nullableAnnotation, typeName);

                if (members is null)
                {
                    members = new List<IMethodSymbol>() { ParseMethod(mockedType.Object), Constructor };
                }

                mockedType.Setup(t => t.GetMembers()).Returns(ImmutableArray.Create<ISymbol>(members.Cast<ISymbol>().ToArray()));

                type = mockedType.Object;
            }

            return new FakeParameterInfo(paramName, type, hasDefaultValue ?? false, defaultValue);
        }

        public static Mock<ITypeSymbol> MockType(NullableAnnotation? nullableAnnotation, string? typeName)
        {
            typeName ??= "int";
            var mockedType = new Mock<ITypeSymbol>(MockBehavior.Strict);
            mockedType.SetupGet(t => t.NullableAnnotation).Returns(nullableAnnotation ?? NullableAnnotation.None);
            mockedType.Setup(t => t.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns(typeName);

            string namespaceName = "System";
            int lastDot = typeName.LastIndexOf('.');
            if (lastDot > 0)
            {
                namespaceName = typeName.Substring(0, lastDot);
            }

            mockedType.SetupGet(t => t.ContainingNamespace).Returns(MockNamespaceSymbol(namespaceName));
            mockedType.SetupGet(t => t.IsValueType).Returns(!(namespaceName.Contains('.') || typeName.Equals("string", StringComparison.OrdinalIgnoreCase)));

            return mockedType;
        }

        private static INamespaceSymbol MockNamespaceSymbol(string namespaceName)
        {
            var mockedNamespace = new Mock<INamespaceSymbol>(MockBehavior.Strict);
            mockedNamespace.Setup(t => t.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns(namespaceName);
            return mockedNamespace.Object;
        }

        public static IArrayTypeSymbol ArrayTypeSymbol(ITypeSymbol? elementType)
        {
            elementType ??= MockType(NullableAnnotation.None, "int").Object;
            var mockedType = new Mock<IArrayTypeSymbol>(MockBehavior.Strict);
            mockedType.SetupGet(t => t.NullableAnnotation).Returns(NullableAnnotation.None);
            mockedType.SetupGet(t => t.ElementType).Returns(elementType);
            mockedType.SetupGet(t => t.IsValueType).Returns(false);
            mockedType.SetupGet(t => t.ContainingNamespace).Returns(MockNamespaceSymbol("System"));
            mockedType.Setup(t => t.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns(elementType.ToDisplayString() + "[]");
            return mockedType.Object;
        }

        public static IMethodSymbol ParseMethod(NullableAnnotation? nullableAnnotation, string? typeName)
        {
            return ParseMethod(MockType(nullableAnnotation, typeName).Object);
        }

        public static IMethodSymbol ParseMethod(ITypeSymbol mockType)
        {
            var mockedParseMethod = new Mock<IMethodSymbol>(MockBehavior.Strict);
            mockedParseMethod.SetupGet(t => t.Name).Returns("Parse");
            mockedParseMethod.SetupGet(t => t.Parameters).Returns(ImmutableArray.Create<IParameterSymbol>(StringParameter));
            mockedParseMethod.SetupGet(t => t.IsStatic).Returns(true);
            mockedParseMethod.SetupGet(t => t.ReturnType).Returns(mockType);
            mockedParseMethod.Setup(t => t.GetAttributes()).Returns(ImmutableArray.Create<AttributeData>());
            return mockedParseMethod.Object;
        }
    }
}
