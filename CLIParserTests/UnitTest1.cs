using ConsoleApp8;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CLIParserTests
{
    [TestClass]
    public class UnitTest1
    {
        private static readonly string[] defaultArgs = [
            "--arg1"
            ,"value1"
            ,"--arg2"
            ,"2"
            ,"--arg-name"
            ,"2"
        ];

        [TestMethod]
        public void BasicString()
        {
            string[] args = [
                ..defaultArgs
            ];

            Parser parser = new();

            Options arguments = parser.Parse(args);

            Assert.AreEqual("value1", arguments.arg1);
        }

        [TestMethod]
        public void BasicInt()
        {
            string[] args = [
                ..defaultArgs
            ];

            Parser parser = new();

            Options arguments = parser.Parse(args);

            Assert.AreEqual(2, arguments.arg2);
        }

        [TestMethod]
        public void MultipartName()
        {
            string[] args = [
                ..defaultArgs
            ];

            Parser parser = new();

            Options arguments = parser.Parse(args);

            Assert.AreEqual(2, arguments.argName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "'--not-an-option' is not a recognized option")]
        public void UnknownOption()
        {
            string[] args = [
                "--not-an-option"
                ,"2"
            ];

            Parser parser = new();

            Options arguments = parser.Parse(args);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Missing required argument: --arg-name")]
        public void MissingOption()
        {
            string[] args = [
                "--arg1"
                ,"value1"
                ,"--arg2"
                ,"2"
            ];

            Parser parser = new();

            Options arguments = parser.Parse(args);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Missing required arguments: --arg2, --arg-name")]
        public void MissingOption2()
        {
            string[] args = [
                "--arg1"
                ,"value1"
            ];

            Parser parser = new();

            Options arguments = parser.Parse(args);
        }

        [TestMethod]
        public void BasicBool()
        {
            string[] args = [
                ..defaultArgs
                ,"--arg3"
            ];

            Parser parser = new();

            Options arguments = parser.Parse(args);

            Assert.AreEqual(true, arguments.arg3);
        }

        [TestMethod]
        public void MissingBool()
        {
            string[] args = [
                ..defaultArgs
            ];

            Parser parser = new();

            Options arguments = parser.Parse(args);

            Assert.AreEqual(false, arguments.arg3);
        }

        [TestMethod]
        public void ArrayArg()
        {
            string[] args = [
                ..defaultArgs
                ,"--arg-array"
                ,"1"
                ,"2"
                ,"3"
            ];

            Parser parser = new();

            Options arguments = parser.Parse(args);

            Assert.AreEqual(3, arguments.argArray.Length);
            Assert.AreEqual(1, arguments.argArray[0]);
            Assert.AreEqual(2, arguments.argArray[1]);
            Assert.AreEqual(3, arguments.argArray[2]);
        }

        [TestMethod]
        public void DefaultValue()
        {
            string[] args = [
                ..defaultArgs
            ];

            Parser parser = new();

            Options arguments = parser.Parse(args);

            Assert.AreEqual("default", arguments.argWithDefault);
        }

        [TestMethod]
        public void EnumTest()
        {
            string[] args = [
                ..defaultArgs
                , "--enum-option"
                , "value2"
            ];

            Parser parser = new();

            Options arguments = parser.Parse(args);

            Assert.AreEqual(SampleEnum.Value2, arguments.enumOption);
        }

        [TestMethod]
        public void EnumCaseTest()
        {
            string[] args = [
                ..defaultArgs
                , "--enum-option"
                , "VALUE2"
            ];

            Parser parser = new();

            Options arguments = parser.Parse(args);

            Assert.AreEqual(SampleEnum.Value2, arguments.enumOption);
        }

        [DataTestMethod]
        [DataRow("-h")]
        [DataRow("-?")]
        [DataRow("--help")]
        public void HelpCall(string helpArg)
        {
            string[] args = [
                helpArg
            ];

            Parser parser = new();

            Options arguments = parser.Parse(args);

            Assert.AreEqual(true, arguments.___ShowHelp___);
        }

        [DataTestMethod]
        [DataRow("myOptionName", "--my-option-name")]
        [DataRow("ADCOption", "--adc-option")]
        public void TestRegex(string input, string expected)
        {
            var rgx = new Regex(@"[A-Z](?=[a-z])");
            string output = "--" + rgx.Replace(input, m => "-" + m.Value.ToLower()).ToLower();

            Assert.AreEqual(expected, output);
        }
    }
}