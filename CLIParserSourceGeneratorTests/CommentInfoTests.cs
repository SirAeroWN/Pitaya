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
    public class CommentInfoTests
    {
        [TestMethod]
        public void SimpleCommentTest()
        {
            string commentLine = "<param name=\"foo\">This is a comment.</param>";
            CommentInfo commentInfo = CommentInfo.Create(commentLine);
            Assert.AreEqual("--foo", commentInfo.OptionName);
            Assert.AreEqual("This is a comment.", commentInfo.Comment);
        }

        [TestMethod]
        public void ComplexNameCommentTest()
        {
            string commentLine = "<param name=\"fooBar\">This is a comment with name and param in it.</param>";
            CommentInfo commentInfo = CommentInfo.Create(commentLine);
            Assert.AreEqual("--foo-bar", commentInfo.OptionName);
            Assert.AreEqual("This is a comment with name and param in it.", commentInfo.Comment);
        }
    }
}
