using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLIParserSourceGenerator
{
    internal class HelpTextGenerator
    {
        private string _assemblyName { get; }

        private string _description { get; }

        private Dictionary<string, OptionInfo> _options { get; }

        private List<CommentInfo> _comments { get; }

        public HelpTextGenerator(string assemblyName, string description, List<OptionInfo> options, List<CommentInfo> comments)
        {
            this._assemblyName = assemblyName;
            this._description = description;
            this._options = options.ToDictionary(o => o.OptionName, o => o);
            this._comments = comments;
        }

        public string Generate()
        {
            StringBuilder sb = new();
            sb.AppendLine("Description:");
            sb.AppendLine($"  {this._description}");
            sb.AppendLine();
            sb.AppendLine($"Usage:");
            sb.AppendLine($"  {this._assemblyName} [options]");
            sb.AppendLine();
            sb.AppendLine("Options:");
            List<(string optionText, string commentText)> lines = new();
            foreach (CommentInfo comment in this._comments)
            {
                string line = $"  {comment.OptionName}";
                var option = this._options[comment.OptionName];
                if (option.Parameter.Type.ToDisplayString() != "bool")
                {
                    line += $" <{option.Parameter.Type.ToDisplayString()}>";
                }

                lines.Add((line, comment.Comment));
            }

            lines.Add(("  -?, -h, --help", "Show help and usage information"));

            int maxWidth = lines.Max(l => l.optionText.Length);
            foreach ((string optionText, string commentText) in lines)
            {
                sb.AppendLine($"{optionText.PadRight(maxWidth)}  {commentText}");
            }

            return sb.ToString();
        }
    }
}
