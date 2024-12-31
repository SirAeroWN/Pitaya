﻿using CLIParserSourceGeneratorTests.Fakes;
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
    public class OverallSourceGeneratorTests
    {
        [TestMethod]
        public void GenerateSourceTest()
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

            var overallSourceGenerator = new OverallSourceGeneratorExposed(mockedmainType.Object, mainReturnType, assemblyName, options, commentLines);
            string source = overallSourceGenerator.GenerateSource();
            string expected = """"
                // auto-generated by SampleGenerator
                #nullable enable
                using System;
                using System.Collections.Generic;

                namespace CLIParserSourceGeneratorTests
                {
                internal class AutoGeneratedProgram
                {
                    static int Main(string[] args)
                    {
                        string helpText = """
                Description:
                  test

                Usage:
                  test [options]

                Options:
                  --a-value <int>  Pass a value
                  -?, -h, --help   Show help and usage information

                """;
                        AutoGeneratedCLIParser parser = new();
                        AutoGeneratedCLIOptionsHolder options = parser.Parse(args);
                        if (options.___ShowHelp___)
                        {
                            Console.WriteLine(helpText);
                            return 0;
                        }

                        return Program.Main(@aValue: options.@aValue);
                    }
                }

                internal class AutoGeneratedCLIParser
                {
                    public AutoGeneratedCLIOptionsHolder Parse(string[] args)
                    {
                        AutoGeneratedCLIOptionsHolder options = new();
                        for (int i = 0; i < args.Length && !options.___ShowHelp___; i++)
                        {
                            string arg = args[i];
                            i++;
                            switch (arg)
                            {
                                case "--a-value":
                                {
                                    options.@aValue = int.Parse(args[i]);
                                    options._aValueValueSet = true;
                                    break;
                                }

                                case "-h":
                                case "-?":
                                case "--help":
                                {
                                    options.___ShowHelp___ = true;
                                    break;
                                }

                                default:
                                {
                                    throw new ArgumentException($"'{arg}' is not a recognized option");
                                }
                            }
                        }

                        if (!options.___ShowHelp___ && !(options._aValueValueSet))
                        {
                            List<string> missing = new();
                            if (!options._aValueValueSet)
                            {
                                missing.Add("--a-value");
                            }

                            throw new ArgumentException($"Missing required argument{(missing.Count > 1 ? "s" : "")}: {string.Join(", ", missing)}");
                        }

                        return options;
                    }
                }

                #pragma warning disable CS8618
                internal class AutoGeneratedCLIOptionsHolder
                {
                    public bool ___ShowHelp___ { get; set; }
                    public int @aValue { get; set; }
                    public bool _aValueValueSet { get; set; }
                }
                }
                """";
            var expectedLines = expected.Split('\n');
            var sourceLines = source.Split('\n');
            foreach (var (expectedLine, sourceLine) in expectedLines.Zip(sourceLines))
            {
                Assert.AreEqual(expectedLine.Trim(), sourceLine.Trim());
            }
        }

        [TestMethod]
        public void GenerateHelpTextTest()
        {
            var mockedmainType = new Mock<INamedTypeSymbol>(MockBehavior.Strict);
            mockedmainType.Setup(t => t.Name).Returns("Program");

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
            string helpText = overallSourceGenerator.GenerateHelpTextExposed();
            string expected = """
                Description:
                  test

                Usage:
                  test [options]

                Options:
                  --a-value <int>  Pass a value
                  -?, -h, --help   Show help and usage information

                """;
            Assert.AreEqual(expected.Replace("\r\n", "\n"), helpText.Replace("\r\n", "\n"));
        }

        [TestMethod]
        public void ExtractDescriptionTest()
        {
            string comments = """
                <summary>
                Say hello
                </summary>
                <param name="aNumber">Leading number</param>
                <param name="name">Their "name"</param>
                """;
            List<string> commentLines = comments.Split('\n').Select(t => t.Trim()).ToList();
            string description = OverallSourceGeneratorExposed.ExtractDescriptionExposed(commentLines);
            Assert.AreEqual("Say hello", description);
        }
    }

    class OverallSourceGeneratorExposed : OverallSourceGenerator
    {
        public OverallSourceGeneratorExposed(INamedTypeSymbol mainType, string mainReturnType, string assemblyName, List<OptionInfo> optionInfos, List<string> commentContent)
            : base(mainType, mainReturnType, assemblyName, optionInfos, commentContent)
        {
        }

        public string GenerateHelpTextExposed()
        {
            return this.GenerateHelpText();
        }

        public static string ExtractDescriptionExposed(List<string> commentContent)
        {
            return ExtractDescription(commentContent);
        }
    }
}
