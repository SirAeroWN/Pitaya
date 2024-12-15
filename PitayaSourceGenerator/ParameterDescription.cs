using CLIParserSourceGenerator;

namespace CLIParserSourceGenerator
{
    internal class ParameterDescription
    {
        public OptionInfo Option { get; }
        public CommentInfo? Comment { get; }

        public ParameterDescription(OptionInfo option, CommentInfo? comment)
        {
            this.Option = option;
            this.Comment = comment;
        }
    }
}