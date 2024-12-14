using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLIParserSourceGenerator
{
    internal class HelpTextGenerator
    {
        private string _assemblyName { get; }

        private string _description { get; }

        private ParameterDescriptionList _parameterDescriptions { get; }

        /// <summary>
        /// Generate help text
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="description"></param>
        /// <param name="options"></param>
        /// <param name="comments"></param>
        public HelpTextGenerator(string assemblyName, string description, List<OptionInfo> options, List<CommentInfo> comments)
        {
            this._assemblyName = assemblyName;
            this._description = description;
            this._parameterDescriptions = ParameterDescriptionList.Create(options, comments);
        }

        public string Generate()
        {
            StringBuilder sb = new();
            if (!string.IsNullOrEmpty(this._description?.Trim()))
            {
                sb.AppendLine("Description:");
                sb.AppendLine($"  {this._description}");
                sb.AppendLine();
            }
            sb.AppendLine($"Usage:");
            sb.AppendLine($"  {this._assemblyName} [options]");
            sb.AppendLine();
            sb.AppendLine("Options:");
            List<(string optionText, string? commentText)> lines = new();
            foreach (ParameterDescription parameterDescription in this._parameterDescriptions)
            {
                string line = $"  {parameterDescription.Option.OptionName}";
                if (parameterDescription.Option.Parameter.Type.ToDisplayString() != "bool")
                {
                    line += $" <{parameterDescription.Option.Parameter.Type.ToDisplayString()}>";
                }

                lines.Add((line, parameterDescription.Comment?.Comment));
            }

            lines.Add(("  -?, -h, --help", "Show help and usage information"));

            int maxWidth = lines.Max(l => l.optionText.Length);
            foreach ((string optionText, string? commentText) in lines)
            {
                if (commentText is null)
                {
                    sb.AppendLine(optionText);
                }
                else
                {
                    sb.AppendLine($"{optionText.PadRight(maxWidth)}  {commentText}");
                }
            }

            return sb.ToString();
        }
    }
}
