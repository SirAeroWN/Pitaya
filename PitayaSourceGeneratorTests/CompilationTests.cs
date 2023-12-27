using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Basic.Reference.Assemblies;
using CLIParserSourceGeneratorTests.Fakes;
using CLIParserSourceGenerator;

namespace CLIParserSourceGeneratorTests
{
    [TestClass]
    public class CompilationTests
    {
        [TestMethod]
        public void CompilesTest()
        {
            #region generate code

            var mockedmainType = new Mock<INamedTypeSymbol>(MockBehavior.Strict);
            mockedmainType.Setup(t => t.Name).Returns("Program");
            var mockedmainNamespace = new Mock<INamespaceSymbol>(MockBehavior.Strict);
            mockedmainNamespace.Setup(t => t.ToDisplayString(It.IsAny<SymbolDisplayFormat?>())).Returns("CLIParserSourceGeneratorTests");
            mockedmainType.SetupGet(t => t.ContainingNamespace).Returns(mockedmainNamespace.Object);

            string mainReturnType = "int";
            string assemblyName = "test";
            List<OptionInfo> options = [
                OptionInfo.Create(FakeParamaterInfo.Create(parameterName: "aValue"))
            ];
            string comments = """
                <summary>
                test
                </summary>
                <param name="aValue">Pass a value</param>
                """;
            List<string> commentLines = comments.Split('\n').Select(t => t.Trim()).ToList();

            var overallSourceGenerator = new OverallSourceGeneratorExposed(mockedmainType.Object, mainReturnType, assemblyName, options, commentLines);
            string source = overallSourceGenerator.GenerateSource();

            string program = """
                class Program
                {
                    /// <summary>
                    /// test
                    /// </summary>
                    /// <param name="aValue">Pass a value</param>
                    public static int Main(int aValue)
                    {
                        return aValue;
                    }
                }
                """;

            #endregion

            // build against netstandard2.0
            List<MetadataReference> references = ReferenceAssemblies.NetStandard20.Cast<MetadataReference>().ToList();

            // compile the generated code
            CSharpCompilation compilation = CSharpCompilation.Create(
                "gen.dll",
                new List<SyntaxTree>() { CSharpSyntaxTree.ParseText(source), CSharpSyntaxTree.ParseText(program) },
                references: references,
                options: CompilationHelpers.CSharpCompilationOptions
            );

            using (var stream = new MemoryStream())
            {
                EmitResult emitResults = compilation.Emit(stream);
                if (!emitResults.Success)
                {
                    Assert.Fail(CompilationHelpers.GetErrors(emitResults));
                }
            }
        }

        [TestMethod]
        public void RunsTest()
        {
            var mockedmainType = new Mock<INamedTypeSymbol>(MockBehavior.Strict);
            mockedmainType.Setup(t => t.Name).Returns("Program");
            var mockedmainNamespace = new Mock<INamespaceSymbol>(MockBehavior.Strict);
            mockedmainNamespace.Setup(t => t.ToDisplayString(It.IsAny<SymbolDisplayFormat?>())).Returns("CLIParserSourceGeneratorTests");
            mockedmainType.SetupGet(t => t.ContainingNamespace).Returns(mockedmainNamespace.Object);

            string mainReturnType = "int";
            string assemblyName = "test";
            List<OptionInfo> options = [
                OptionInfo.Create(FakeParamaterInfo.Create(parameterName: "aValue"))
            ];
            string comments = """
                <summary>
                test
                </summary>
                <param name="aValue">Pass a value</param>
                """;
            List<string> commentLines = comments.Split('\n').Select(t => t.Trim()).ToList();

            string program = """
                class Program
                {
                    /// <summary>
                    /// test
                    /// </summary>
                    /// <param name="aValue">Pass a value</param>
                    public static int Main(int aValue)
                    {
                        return aValue;
                    }
                }
                """;

            Func<string[], int> generatedMethod = CompilationHelpers.CompileProgram(program, mockedmainType.Object, mainReturnType, assemblyName, options, commentLines);

            Assert.AreEqual(1, generatedMethod([ "--a-value", "1" ]));
        }
    }
}
