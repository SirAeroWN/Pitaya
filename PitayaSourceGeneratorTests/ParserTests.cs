using Basic.Reference.Assemblies;
using CLIParserSourceGeneratorTests.Fakes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CLIParserSourceGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CLIParserSourceGeneratorTests
{
    [TestClass]
    public class ParserTests
    {
        private static INamedTypeSymbol? _mainType;
        private static INamedTypeSymbol MainType
        {
            get
            {
                if (_mainType is null)
                {
                    var mockedmainType = new Mock<INamedTypeSymbol>(MockBehavior.Strict);
                    mockedmainType.Setup(t => t.Name).Returns("Program");
                    var mockedmainNamespace = new Mock<INamespaceSymbol>(MockBehavior.Strict);
                    mockedmainNamespace.Setup(t => t.ToDisplayString(It.IsAny<SymbolDisplayFormat?>())).Returns("CLIParserSourceGeneratorTests");
                    mockedmainType.SetupGet(t => t.ContainingNamespace).Returns(mockedmainNamespace.Object);
                    _mainType = mockedmainType.Object;
                }

                return _mainType;
            }
        }

        private static string AssemblyName = "test";

        [TestMethod]
        public void SimpleTest()
        {
            string mainReturnType = "int";
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

            string program = $$"""
                class Program
                {
                    {{string.Join("\n", commentLines.Select(cl => "/// " + cl))}}
                    public static int Main(int aValue)
                    {
                        return aValue;
                    }
                }
                """;

            Func<string[], int?> generatedMethod = CompilationHelpers.CompileProgram(program, MainType, mainReturnType, AssemblyName, options, commentLines);

            Assert.AreEqual(1, generatedMethod(["--a-value", "1"]));
        }

        [TestMethod]
        public void SimpleStringTest()
        {
            string mainReturnType = "int";
            List<OptionInfo> options = [
                OptionInfo.Create(FakeParameterInfo.Create(parameterName: "aString", typeName: "string"))
            ];
            string comments = """
                <summary>
                test
                </summary>
                <param name="aString">Pass a value</param>
                """;
            List<string> commentLines = comments.Split('\n').Select(t => t.Trim()).ToList();

            string program = $$"""
                class Program
                {
                    {{string.Join("\n", commentLines.Select(cl => "/// " + cl))}}
                    public static int Main(string aString)
                    {
                        return aString == "hello there" ? 0 : 1;
                    }
                }
                """;

            Func<string[], int?> generatedMethod = CompilationHelpers.CompileProgram(program, MainType, mainReturnType, AssemblyName, options, commentLines);

            Assert.AreEqual(0, generatedMethod(["--a-string", "hello there"]));
        }

        [TestMethod]
        public void QuotedStringTest()
        {
            string mainReturnType = "int";
            List<OptionInfo> options = [
                OptionInfo.Create(FakeParameterInfo.Create(parameterName: "aString", typeName: "string"))
            ];
            string comments = """
                <summary>
                test
                </summary>
                <param name="aString">Pass a value</param>
                """;
            List<string> commentLines = comments.Split('\n').Select(t => t.Trim()).ToList();

            string program = $$"""
                class Program
                {
                    {{string.Join("\n", commentLines.Select(cl => "/// " + cl))}}
                    public static int Main(string aString)
                    {
                        return aString == "--hello there" ? 0 : 1;
                    }
                }
                """;

            Func<string[], int?> generatedMethod = CompilationHelpers.CompileProgram(program, MainType, mainReturnType, AssemblyName, options, commentLines);

            Assert.AreEqual(0, generatedMethod(["--a-string", "--hello there"]));
        }

        [TestMethod]
        public void MultipleArgsTest()
        {
            string mainReturnType = "int";
            List<OptionInfo> options = [
                OptionInfo.Create(FakeParameterInfo.Create(parameterName: "aValue"))
                , OptionInfo.Create(FakeParameterInfo.Create(parameterName: "aValueTwo", typeName: "long"))
                , OptionInfo.Create(FakeParameterInfo.Create(parameterName: "aValueThree"))
                , OptionInfo.Create(FakeParameterInfo.Create(parameterName: "aString", typeName: "string"))
            ];
            string comments = """
                <summary>
                test
                </summary>
                <param name="aValue">Pass a value</param>
                <param name="aValueTwo">Pass a value</param>
                <param name="aValueThree">Pass a value</param>
                <param name="aString">Pass a value</param>
                """;
            List<string> commentLines = comments.Split('\n').Select(t => t.Trim()).ToList();

            string program = $$"""
                class Program
                {
                    {{string.Join("\n", commentLines.Select(cl => "/// " + cl))}}
                    public static int Main(int aValue, long aValueTwo, int aValueThree, string aString)
                    {
                        bool valid = aValue == 1 && aValueTwo == 2147484647 && aValueThree == 3 && aString == "hello there";
                        return valid ? 0 : 1;
                    }
                }
                """;

            Func<string[], int?> generatedMethod = CompilationHelpers.CompileProgram(program, MainType, mainReturnType, AssemblyName, options, commentLines);

            // 2147484647 is int.MaxValue + 1000
            Assert.AreEqual(0, generatedMethod(["--a-value", "1", "--a-value-two", "2147484647", "--a-value-three", "3", "--a-string", "hello there"]));
        }

        [TestMethod]
        public void DateTimeTest()
        {
            string mainReturnType = "int";
            List<OptionInfo> options = [
                OptionInfo.Create(FakeParameterInfo.Create(parameterName: "aDate", typeName: "System.DateTime"))
            ];
            string comments = """
                <summary>
                test
                </summary>
                <param name="aDate">Pass a value</param>
                """;
            List<string> commentLines = comments.Split('\n').Select(t => t.Trim()).ToList();

            string program = $$"""
                using System;
                class Program
                {
                    {{string.Join("\n", commentLines.Select(cl => "/// " + cl))}}
                    public static int Main(DateTime aDate)
                    {
                        var d = DateTime.Parse("January 3, 2021");
                        return d == aDate ? 0 : 1;
                    }
                }
                """;

            Func<string[], int?> generatedMethod = CompilationHelpers.CompileProgram(program, MainType, mainReturnType, AssemblyName, options, commentLines);

            Assert.AreEqual(0, generatedMethod(["--a-date", "January 3, 2021"]));
        }

        [TestMethod]
        public void UriTest()
        {
            string mainReturnType = "int";
            List<OptionInfo> options = [
                OptionInfo.Create(FakeParameterInfo.Create(parameterName: "aUri", typeName: "System.Uri"))
            ];
            string comments = """
                <summary>
                test
                </summary>
                <param name="aUri">Pass a value</param>
                """;
            List<string> commentLines = comments.Split('\n').Select(t => t.Trim()).ToList();

            string program = $$"""
                using System;
                class Program
                {
                    {{string.Join("\n", commentLines.Select(cl => "/// " + cl))}}
                    public static int Main(Uri aUri)
                    {
                        return aUri.DnsSafeHost == "www.alarm.com" ? 0 : 1;
                    }
                }
                """;

            Func<string[], int?> generatedMethod = CompilationHelpers.CompileProgram(program, MainType, mainReturnType, AssemblyName, options, commentLines);

            Assert.AreEqual(0, generatedMethod(["--a-uri", "https://www.Alarm.com/commercial-business"]));
        }

        [TestMethod]
        public void FileInfoTest()
        {
            string mainReturnType = "int";
            string paramName = "file";
            List<OptionInfo> options = [
                OptionInfo.Create(FakeParameterInfo.Create(parameterName: paramName, typeName: "System.IO.FileInfo", members: [FakeParameterInfo.Constructor]))
            ];
            string comments = $$"""
                <summary>
                test
                </summary>
                <param name="{{paramName}}">Pass a value</param>
                """;
            List<string> commentLines = comments.Split('\n').Select(t => t.Trim()).ToList();

            string program = $$"""
                using System;
                using System.IO;
                class Program
                {
                    {{string.Join("\n", commentLines.Select(cl => "/// " + cl))}}
                    public static int Main(FileInfo {{paramName}})
                    {
                        return {{paramName}}.Extension == ".txt" ? 0 : 1;
                    }
                }
                """;

            Func<string[], int?> generatedMethod = CompilationHelpers.CompileProgram(program, MainType, mainReturnType, AssemblyName, options, commentLines);

            Assert.AreEqual(0, generatedMethod(["--file", "C:/temp.txt"]));
        }

        [TestMethod]
        public void ArrayTest()
        {
            string mainReturnType = "int";
            string paramName = "anArray";
            List<OptionInfo> options = [
                OptionInfo.Create(FakeParameterInfo.Create(parameterName: paramName, type: FakeParameterInfo.ArrayTypeSymbol(null)))
            ];
            string comments = $$"""
                <summary>
                test
                </summary>
                <param name="{{paramName}}">Pass a value</param>
                """;
            List<string> commentLines = comments.Split('\n').Select(t => t.Trim()).ToList();

            string program = $$"""
                using System;
                class Program
                {
                    {{string.Join("\n", commentLines.Select(cl => "/// " + cl))}}
                    public static int Main(int[] {{paramName}})
                    {
                        return {{paramName}}.Length == 3 && {{paramName}}[0] == 1 && {{paramName}}[1] == 2 && {{paramName}}[2] == 3 ? 0 : 1;
                    }
                }
                """;

            Func<string[], int?> generatedMethod = CompilationHelpers.CompileProgram(program, MainType, mainReturnType, AssemblyName, options, commentLines);

            Assert.AreEqual(0, generatedMethod(["--an-array", "1", "2", "3"]));
        }

        [TestMethod]
        public void ArrayWithOtherOptionsTest()
        {
            string mainReturnType = "int";
            string paramName = "anArray";
            List<OptionInfo> options = [
                OptionInfo.Create(FakeParameterInfo.Create(parameterName: paramName, type: FakeParameterInfo.ArrayTypeSymbol(null)))
                , OptionInfo.Create(FakeParameterInfo.Create(parameterName: "anInt"))
            ];
            string comments = $$"""
                <summary>
                test
                </summary>
                <param name="{{paramName}}">Pass a value</param>
                <param name="anInt">Pass a value</param>
                """;
            List<string> commentLines = comments.Split('\n').Select(t => t.Trim()).ToList();

            string program = $$"""
                using System;
                class Program
                {
                    {{string.Join("\n", commentLines.Select(cl => "/// " + cl))}}
                    public static int Main(int[] {{paramName}}, int anInt)
                    {
                        return {{paramName}}.Length == 3 && anInt == 42 ? 0 : 1;
                    }
                }
                """;

            Func<string[], int?> generatedMethod = CompilationHelpers.CompileProgram(program, MainType, mainReturnType, AssemblyName, options, commentLines);

            Assert.AreEqual(0, generatedMethod(["--an-array", "1", "2", "3", "--an-int", "42"]));
        }
    }
}
