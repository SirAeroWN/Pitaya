using CLIParserSourceGeneratorTests.Fakes;
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
    public class HelpTextGeneratorTests
    {
        [TestMethod]
        public void GenerateTest()
        {
            string[] comments = [
                "<param name=\"foo\">This is a comment.</param>",
                "<param name=\"fooBar\">This is another comment.</param>"
            ];

            List<CommentInfo> commentInfos = comments.Select(t => CommentInfo.Create(t)).ToList();

            List<OptionInfo> options = [
                OptionInfo.Create(FakeParameterInfo.Create(parameterName: "foo"))
                , OptionInfo.Create(FakeParameterInfo.Create(parameterName: "fooBar"))
            ];

            var generator = new HelpTextGenerator("test", "This is a description", options, commentInfos);
            string source = generator.Generate();
            string expected = """
                Description:
                  This is a description

                Usage:
                  test [options]

                Options:
                  --foo <int>      This is a comment.
                  --foo-bar <int>  This is another comment.
                  -?, -h, --help   Show help and usage information

                """;

            Assert.AreEqual(expected, source);
        }

        [TestMethod]
        public void GenerateHelpWithoutCommentsTest()
        {
            List<OptionInfo> options = [
                OptionInfo.Create(FakeParameterInfo.Create(parameterName: "foo"))
                , OptionInfo.Create(FakeParameterInfo.Create(parameterName: "fooBar"))
            ];

            var generator = new HelpTextGenerator("test", "", options, new List<CommentInfo>());
            string source = generator.Generate();
            string expected = """
                Usage:
                  test [options]

                Options:
                  --foo <int>
                  --foo-bar <int>
                  -?, -h, --help   Show help and usage information

                """;

            Assert.AreEqual(expected, source);
        }
    }
}
