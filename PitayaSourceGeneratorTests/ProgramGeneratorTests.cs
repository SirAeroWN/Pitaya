﻿using CLIParserSourceGeneratorTests.Fakes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CLIParserSourceGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLIParserSourceGeneratorTests
{
    [TestClass]
    public class ProgramGeneratorTests
    {
        private static readonly string _optionsClassName = "AutoGeneratedCLIOptionsHolder";
        private static readonly string _parserClassName = "AutoGeneratedCLIParser";
        private static readonly string _helpText = "This is the help text.";

        [TestMethod]
        public void GenerateSourceTest()
        {
            List<OptionInfo> options = [
                OptionInfo.Create(FakeParameterInfo.Create(parameterName: "aValue"))
                , OptionInfo.Create(FakeParameterInfo.Create(parameterName: "anotherValue", typeName: "uint", hasDefaultValue: true, defaultValue: 1))
                , OptionInfo.Create(FakeParameterInfo.Create(parameterName: "yetAnotherValue", typeName: "string"))
            ];
            var generator = new ProgramGenerator(options, _optionsClassName, _parserClassName, _helpText, MainReturnTypeEnum.Int);
            string source = generator.GenerateSource();
            string expected = $$""""
                internal class AutoGeneratedProgram
                {
                    static int Main(string[] args)
                    {
                        string helpText = """
                {{_helpText}}
                """;
                        {{_parserClassName}} parser = new();
                        {{_optionsClassName}} options = parser.Parse(args);
                        if (options.___ShowHelp___)
                        {
                            Console.WriteLine(helpText);
                            return 0;
                        }

                        return Program.Main(aValue: options.aValue, anotherValue: options.anotherValue, yetAnotherValue: options.yetAnotherValue);
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

        [DataTestMethod]
        [DataRow(MainReturnTypeEnum.Void, "return;")]
        [DataRow(MainReturnTypeEnum.Int, "return 0;")]
        [DataRow(MainReturnTypeEnum.Task, "return;")]
        [DataRow(MainReturnTypeEnum.TaskInt, "return 0;")]
        public void GenerateHelpReturnStyleTest(MainReturnTypeEnum returnTypeEnum, string expected)
        {
            var generator = new ProgramGeneratorExposed(new List<OptionInfo>(), _optionsClassName, _parserClassName, _helpText, returnTypeEnum);
            string source = generator.GenerateHelpReturnStyleExposed();
            Assert.AreEqual(expected, source);
        }

        [DataTestMethod]
        [DataRow(MainReturnTypeEnum.Void, "")]
        [DataRow(MainReturnTypeEnum.Int, "return ")]
        [DataRow(MainReturnTypeEnum.Task, "await ")]
        [DataRow(MainReturnTypeEnum.TaskInt, "return await ")]
        public void GenerateReturnStyleTest(MainReturnTypeEnum returnTypeEnum, string expected)
        {
            var generator = new ProgramGeneratorExposed(new List<OptionInfo>(), _optionsClassName, _parserClassName, _helpText, returnTypeEnum);
            string source = generator.GenerateReturnStyleExposed();
            Assert.AreEqual(expected, source);
        }

        [DataTestMethod]
        [DataRow(MainReturnTypeEnum.Void, "static void Main()")]
        [DataRow(MainReturnTypeEnum.Int, "static int Main()")]
        [DataRow(MainReturnTypeEnum.Task, "static async Task Main()")]
        [DataRow(MainReturnTypeEnum.TaskInt, "static async Task<int> Main()")]
        public void GenerateMethodStubTest(MainReturnTypeEnum returnTypeEnum, string expected)
        {
            var generator = new ProgramGeneratorExposed(new List<OptionInfo>(), _optionsClassName, _parserClassName, _helpText, returnTypeEnum);
            string source = generator.GenerateMethodStubExposed().NormalizeWhitespace().ToFullString();
            Assert.AreEqual(expected, source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Unexpected return type ''.")]
        public void GenerateMethodStub_InvalidReturn_Test()
        {
            var generator = new ProgramGeneratorExposed(new List<OptionInfo>(), _optionsClassName, _parserClassName, _helpText, (MainReturnTypeEnum)111);
            string source = generator.GenerateMethodStubExposed().NormalizeWhitespace().ToFullString();
        }

        [TestMethod]
        public void GenerateVoidMethodStubTest()
        {
            var generator = new ProgramGeneratorExposed(new List<OptionInfo>(), _optionsClassName, _parserClassName, _helpText, MainReturnTypeEnum.Void);
            string source = generator.GenerateVoidMethodStubExposed().NormalizeWhitespace().ToFullString();
            string expected = "static void Main()";
            Assert.AreEqual(expected, source);
        }

        [TestMethod]
        public void GenerateIntMethodStubTest()
        {
            var generator = new ProgramGeneratorExposed(new List<OptionInfo>(), _optionsClassName, _parserClassName, _helpText, MainReturnTypeEnum.Int);
            string source = generator.GenerateIntMethodStubExposed().NormalizeWhitespace().ToFullString();
            string expected = "static int Main()";
            Assert.AreEqual(expected, source);
        }

        [TestMethod]
        public void GenerateTaskMethodStubTest()
        {
            var generator = new ProgramGeneratorExposed(new List<OptionInfo>(), _optionsClassName, _parserClassName, _helpText, MainReturnTypeEnum.Task);
            string source = generator.GenerateTaskMethodStubExposed().NormalizeWhitespace().ToFullString();
            string expected = "static async Task Main()";
            Assert.AreEqual(expected, source);
        }

        [TestMethod]
        public void GenerateTaskIntMethodStubTest()
        {
            var generator = new ProgramGeneratorExposed(new List<OptionInfo>(), _optionsClassName, _parserClassName, _helpText, MainReturnTypeEnum.TaskInt);
            string source = generator.GenerateTaskIntMethodStubExposed().NormalizeWhitespace().ToFullString();
            string expected = "static async Task<int> Main()";
            Assert.AreEqual(expected, source);
        }
    }

    class ProgramGeneratorExposed : ProgramGenerator
    {
        public ProgramGeneratorExposed(List<OptionInfo> options, string optionsClassName, string parserClassName, string helpText, MainReturnTypeEnum mainReturnType)
            : base(options, optionsClassName, parserClassName, helpText, mainReturnType)
        {
        }

        public string GenerateHelpReturnStyleExposed()
        {
            return this.GenerateHelpReturnStyle();
        }

        public string GenerateReturnStyleExposed()
        {
            return this.GenerateReturnStyle();
        }

        public MethodDeclarationSyntax GenerateMethodStubExposed()
        {
            return this.GenerateMethodStub();
        }

        public MethodDeclarationSyntax GenerateVoidMethodStubExposed()
        {
            return this.GenerateVoidMethodStub();
        }

        public MethodDeclarationSyntax GenerateIntMethodStubExposed()
        {
            return this.GenerateIntMethodStub();
        }

        public MethodDeclarationSyntax GenerateTaskMethodStubExposed()
        {
            return this.GenerateTaskMethodStub();
        }

        public MethodDeclarationSyntax GenerateTaskIntMethodStubExposed()
        {
            return this.GenerateTaskIntMethodStub();
        }
    }
}