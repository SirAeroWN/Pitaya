﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using CLIParserSourceGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLIParserSourceGeneratorTests
{
    [TestClass]
    public class FileGeneratorTests
    {
        [TestMethod]
        public void GenerateSourceTest()
        {
            string @namespace = "CLIParserSourceGeneratorTests";
            string programSource = "internal class Program { static void Main(string[] args) { } }";
            string parserSource = "internal class Parser { }";
            string optionsSource = "internal class Options { public int aValue { get; set; } }";
            var fileGenerator = new FileGenerator(@namespace, programSource, parserSource, optionsSource, new());
            string source = fileGenerator.GenerateSource();
            string expected = $$""""
                // auto-generated by SampleGenerator
                using System;
                using System.Collections.Generic;
                using System.Threading.Tasks;

                namespace {{@namespace}}
                {
                {{programSource}}

                {{parserSource}}

                {{optionsSource}}
                }
                """";
            var expectedLines = expected.Split('\n');
            var sourceLines = source.Split('\n');
            foreach (var (expectedLine, sourceLine) in expectedLines.Zip(sourceLines))
            {
                Assert.AreEqual(expectedLine.Trim(), sourceLine.Trim());
            }
        }
    }
}
