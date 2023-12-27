using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLIParserSourceGenerator
{
    internal class OverallSourceGenerator
    {
        private INamedTypeSymbol _mainType { get; }
        private string _mainReturnType { get; }
        private string _assemblyName { get; }
        private List<OptionInfo> _optionInfos { get; }
        private List<string> _commentContent { get; }

        public string TypeName { get; }

        public OverallSourceGenerator(INamedTypeSymbol mainType, string mainReturnType, string assemblyName, List<OptionInfo> optionInfos, List<string> commentContent)
        {
            this._mainType = mainType;
            this._mainReturnType = mainReturnType;
            this._assemblyName = assemblyName;
            this._optionInfos = optionInfos;
            this._commentContent = commentContent;
            this.TypeName = mainType.Name;
        }

        public string GenerateSource()
        {
            OptionsClassGenerator optionClassGenerator = new OptionsClassGenerator(this._optionInfos);
            string optionClassSource = optionClassGenerator.GenerateSource();

            ParserGenerator parserGenerator = new ParserGenerator(this._optionInfos, optionClassGenerator.ClassName);
            string parserClassSource = parserGenerator.GenerateSource();

            string helpText = this.GenerateHelpText();

            ProgramGenerator programGenerator = new ProgramGenerator(this._optionInfos, optionClassGenerator.ClassName, parserGenerator.ClassName, helpText, this._mainReturnType.ToMainReturnTypeEnum());
            string programClassSource = programGenerator.GenerateSource();

            FileGenerator fileGenerator = new FileGenerator(this._mainType.ContainingNamespace.ToDisplayString(), programClassSource, parserClassSource, optionClassSource, this._optionInfos);
            return fileGenerator.GenerateSource();
        }

        protected string GenerateHelpText()
        {
            IEnumerable<CommentInfo> comments = this._commentContent
                            .Where(t => t.StartsWith("<param name"))
                            .Select(t => CommentInfo.Create(t));
            string description = ExtractDescription(this._commentContent);

            HelpTextGenerator helpTextGenerator = new HelpTextGenerator(this._assemblyName!, description, this._optionInfos, comments.ToList());
            string helpText = helpTextGenerator.Generate();
            return helpText;
        }

        protected static string ExtractDescription(List<string> commentContent)
        {
            List<string> descriptionLines = commentContent.SkipWhile(t => !t.StartsWith("<summary>")).Skip(1).TakeWhile(t => !t.StartsWith("</summary>")).ToList();
            string description = string.Join("\n", descriptionLines);
            return description;
        }
    }
}
