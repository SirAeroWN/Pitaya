using Microsoft.VisualStudio.TestTools.UnitTesting;
using CLIParserSourceGenerator;

namespace CLIParserSourceGeneratorTests
{
    [TestClass]
    public class UtilitiesTests
    {
        [DataTestMethod]
        [DataRow("name", "--name")]
        [DataRow("aName", "--a-name")]
        [DataRow("ABCValue", "--abc-value")]
        [DataRow("FirstChar", "--first-char")]
        [DataRow("userID", "--user-id")]
        public void OptionifyTest(string paramName, string expected)
        {
            string actual = Utilities.Optionify(paramName);
            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod]
        [DataRow("name", "name")]
        [DataRow("aName", "aName")]
        [DataRow("ABCValue", "ABCValue")]
        public void PropertyifyTest(string paramName, string expected)
        {
            string actual = Utilities.Propertyify(paramName);
            Assert.AreEqual(expected, actual);
        }
    }
}