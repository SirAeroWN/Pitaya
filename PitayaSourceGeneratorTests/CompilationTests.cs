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
using System.Reflection;
using System.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Collections.Immutable;

namespace CLIParserSourceGeneratorTests
{
    [TestClass]
    public class CompilationTests
    {
        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create(
                "gen.dll",
                new List<SyntaxTree>() { CSharpSyntaxTree.ParseText(source) },
                references: ReferenceAssemblies.NetStandard20.Cast<MetadataReference>().ToList(),
                options: CompilationHelpers.CSharpCompilationOptions
            );

        private static void CompileAndRunGenerator(string program, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics)
        {
            var compilation = CreateCompilation(program);
            var generator = new PitayaSourceGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out diagnostics);
        }

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
                OptionInfo.Create(FakeParameterInfo.Create(parameterName: "aValue"))
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
                OptionInfo.Create(FakeParameterInfo.Create(parameterName: "aValue"))
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

        [TestMethod]
        public void FileInfoCompilesTest()
        {
            string program = """
                using System;
                using System.IO;

                namespace ConsoleApp8
                {
                    class Program
                    {
                        /// <summary>
                        /// My sample program
                        /// </summary>
                        /// <param name="required"></param>
                        /// <param name="optionalFile"></param>
                        /// <param name="hasADefault"></param>
                        public static void Main(int required, FileInfo? optionalFile, string hasADefault = "My great default")
                        {
                            Console.WriteLine($"required: {required}");
                            Console.WriteLine($"optionalFile was {(optionalFile == null ? "not passed" : "passed")}");
                            Console.WriteLine($"hasADefault: {hasADefault}");
                        }
                    }
                }
                """;
            CompileAndRunGenerator(program, out Compilation outputCompilation, out ImmutableArray<Diagnostic>  diagnostics);

            // We can now assert things about the resulting compilation:
            Assert.IsTrue(diagnostics.IsEmpty); // there were no diagnostics created by the generators
            Assert.IsTrue(outputCompilation.SyntaxTrees.Count() == 2); // we have two syntax trees, the original 'user' provided one, and the one added by the generator
            string generatedCode = outputCompilation.SyntaxTrees.ToList()[1].GetText().ToString();
            System.Collections.Immutable.ImmutableArray<Diagnostic> outputDiagnostics = outputCompilation.GetDiagnostics();
            Assert.IsTrue(outputDiagnostics.IsEmpty); // verify the compilation with the added source has no diagnostics
        }
    }
}
