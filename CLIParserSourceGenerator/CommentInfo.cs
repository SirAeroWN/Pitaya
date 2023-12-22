using System;
using System.Collections.Generic;
using System.Text;

namespace CLIParserSourceGenerator
{
    internal class CommentInfo
    {
        public string OptionName { get; }
        public string Comment { get; }

        public CommentInfo(string optionName, string comment)
        {
            this.OptionName = optionName;
            this.Comment = comment;
        }

        public override string ToString()
        {
            return $"OptionName: {OptionName}, Comment: {Comment}";
        }

        public static CommentInfo Create(string commentText)
        {
            int endOfOpeningTag = commentText.IndexOf('>');
            if (endOfOpeningTag < 0)
            {
                throw new ArgumentException($"Comment '{commentText}' does not contain an opening tag.");
            }

            int startOfClosingTag = commentText.IndexOf("</param>", endOfOpeningTag);
            if (startOfClosingTag < 0)
            {
                throw new ArgumentException($"Comment '{commentText}' does not contain a closing tag.");
            }

            string comment = commentText.Substring(endOfOpeningTag + 1, startOfClosingTag - endOfOpeningTag - 1).Trim();

            Lazy<string> missingNameErrorText = new(() => $"Comment '{commentText}' does not contain a name.");
            int startOfName = commentText.IndexOf("name=\"");
            if (startOfName < 0)
            {
                throw new ArgumentException(missingNameErrorText.Value);
            }

            int endOfName = commentText.IndexOf("\"", startOfName + 6);
            if (endOfName < 0)
            {
                throw new ArgumentException(missingNameErrorText.Value);
            }

            string optionName = commentText.Substring(startOfName + 6, endOfName - startOfName - 6);
            return new CommentInfo(Utilities.Optionify(optionName), comment);
        }
    }
}
