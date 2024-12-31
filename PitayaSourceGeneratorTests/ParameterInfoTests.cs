using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CLIParserSourceGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLIParserSourceGeneratorTests
{
    [TestClass]
    public class ParameterInfoTests
    {
        [TestMethod]
        public void NormalParameterTest()
        {
            string paramName = "aValue";
            string typeName = "int";
            var mockedParameterType = new Mock<IParameterSymbol>(MockBehavior.Strict);
            mockedParameterType.SetupGet(p => p.Name).Returns(paramName);
            var mockedType = new Mock<ITypeSymbol>(MockBehavior.Strict);
            mockedType.SetupGet(t => t.NullableAnnotation).Returns(NullableAnnotation.None);
            mockedType.SetupGet(p => p.Name).Returns(typeName);
            mockedParameterType.SetupGet(p => p.Type).Returns(mockedType.Object);
            mockedParameterType.SetupGet(p => p.HasExplicitDefaultValue).Returns(false);
            var parameterInfo = ParameterInfo.Create(mockedParameterType.Object);

            Assert.AreEqual(paramName, parameterInfo.ParameterName);
            Assert.AreEqual(mockedType.Object, parameterInfo.Type);
            Assert.IsFalse(parameterInfo.IsNullable);
            Assert.IsFalse(parameterInfo.HasDefaultValue);
            Assert.IsNull(parameterInfo.DefaultValue);
        }

        [TestMethod]
        public void NullableParameterTest()
        {
            string paramName = "aValue";
            string typeName = "int?";
            var mockedParameterType = new Mock<IParameterSymbol>(MockBehavior.Strict);
            mockedParameterType.SetupGet(p => p.Name).Returns(paramName);
            var mockedType = new Mock<ITypeSymbol>(MockBehavior.Strict);
            mockedType.SetupGet(t => t.NullableAnnotation).Returns(NullableAnnotation.Annotated);
            mockedType.SetupGet(p => p.Name).Returns(typeName);
            mockedParameterType.SetupGet(p => p.Type).Returns(mockedType.Object);
            mockedParameterType.SetupGet(p => p.HasExplicitDefaultValue).Returns(false);
            var parameterInfo = ParameterInfo.Create(mockedParameterType.Object);

            Assert.IsTrue(parameterInfo.IsNullable);
        }

        [TestMethod]
        public void DefaultParameterTest()
        {
            string paramName = "aValue";
            var mockedParameterType = new Mock<IParameterSymbol>(MockBehavior.Strict);
            mockedParameterType.SetupGet(p => p.Name).Returns(paramName);
            var mockedType = new Mock<ITypeSymbol>(MockBehavior.Strict);
            mockedType.SetupGet(t => t.NullableAnnotation).Returns(NullableAnnotation.None);
            mockedParameterType.SetupGet(p => p.Type).Returns(mockedType.Object);
            mockedParameterType.SetupGet(p => p.HasExplicitDefaultValue).Returns(true);
            mockedParameterType.SetupGet(p => p.ExplicitDefaultValue).Returns(42);
            var parameterInfo = ParameterInfo.Create(mockedParameterType.Object);

            Assert.IsTrue(parameterInfo.HasDefaultValue);
            Assert.IsNotNull(parameterInfo.DefaultValue);
            Assert.AreEqual(42, parameterInfo.DefaultValue);
        }
    }
}
