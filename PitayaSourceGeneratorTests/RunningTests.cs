using CLIParserSourceGenerator;
using CLIParserSourceGeneratorTests.Fakes;
using CLIParserSourceGeneratorTests;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.IO;

namespace PitayaSourceGeneratorTests
{
    [TestClass]
    public class RunningTests
    {
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

            Func<string[], int?> generatedMethod = CompilationHelpers.CompileProgram(program, mockedmainType.Object, mainReturnType, assemblyName, options, commentLines);

            Assert.AreEqual(1, generatedMethod(["--a-value", "1"]));
        }

        [TestMethod]
        public void FileInfoRunsTest()
        {
            string program = """
                using System;
                using System.IO;

                namespace CLIParserSourceGeneratorTests
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
            CompilationHelpers.CompileAndRunGenerator(program, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics);

            var runnable = CompilationHelpers.CreateRunnable(outputCompilation);
            using (StringWriter sw = new StringWriter())
            {
                // redirect stdout
                Console.SetOut(sw);
                // run
                var result = runnable(["--required", "1"]);
                // void return type should return null
                Assert.AreEqual(null, result);
                // normalize output
                var output = sw.ToString().Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
                List<string> expectedOutput = new List<string>() { "required: 1", "optionalFile was not passed", "hasADefault: My great default" };
                // compare
                Assert.AreEqual(expectedOutput.Count, output.Count);
                for (int i = 0; i < expectedOutput.Count; i++)
                {
                    Assert.AreEqual(expectedOutput[i], output[i]);
                }
            }
        }

        [TestMethod]
        public void FileInfoRuns_FilePassed_Test()
        {
            string program = """
                using System;
                using System.IO;

                namespace CLIParserSourceGeneratorTests
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
            CompilationHelpers.CompileAndRunGenerator(program, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics);

            var runnable = CompilationHelpers.CreateRunnable(outputCompilation);
            using (StringWriter sw = new StringWriter())
            {
                // redirect stdout
                Console.SetOut(sw);
                // run
                var result = runnable(["--required", "1", "--optional-file", "totally_real.txt"]);
                // void return type should return null
                Assert.AreEqual(null, result);
                // normalize output
                var output = sw.ToString().Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
                List<string> expectedOutput = new List<string>() { "required: 1", "optionalFile was passed", "hasADefault: My great default" };
                // compare
                Assert.AreEqual(expectedOutput.Count, output.Count);
                for (int i = 0; i < expectedOutput.Count; i++)
                {
                    Assert.AreEqual(expectedOutput[i], output[i]);
                }
            }
        }

        [TestMethod]
        public void ReservedNames_Test()
        {
            string program = """
                using System;
                using System.IO;

                namespace CLIParserSourceGeneratorTests
                {
                    class Program
                    {
                        public static void Main(string @class, string @namespace)
                        {
                            Console.WriteLine($"class: {@class}");
                            Console.WriteLine($"namespace: {@namespace}");
                        }
                    }
                }
                """;
            CompilationHelpers.CompileAndRunGenerator(program, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics);

            var runnable = CompilationHelpers.CreateRunnable(outputCompilation);
            using (StringWriter sw = new StringWriter())
            {
                // redirect stdout
                Console.SetOut(sw);
                // run
                var result = runnable(["--class", "className", "--namespace", "namespace.name"]);
                // void return type should return null
                Assert.AreEqual(null, result);
                // normalize output
                var output = sw.ToString().Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
                List<string> expectedOutput = new List<string>() { "class: className", "namespace: namespace.name" };
                // compare
                Assert.AreEqual(expectedOutput.Count, output.Count);
                for (int i = 0; i < expectedOutput.Count; i++)
                {
                    Assert.AreEqual(expectedOutput[i], output[i]);
                }
            }
        }
    }
}
