﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLIParserSourceGenerator
{
    internal class FileGenerator
    {
        private string _namespace { get; }

        private string _programSource { get; }

        private string _parserSource { get; }

        private string _optionsSource { get; }

        private List<OptionInfo> _options { get; }

        public FileGenerator(string @namespace, string programSource, string parserSource, string optionsSource, List<OptionInfo> options)
        {
            this._namespace = @namespace;
            this._programSource = programSource;
            this._parserSource = parserSource;
            this._optionsSource = optionsSource;
            this._options = options;
        }

        public string GenerateSource()
        {
            HashSet<string> namespaces = [
                "System"
                , "System.Collections.Generic"
                //, "System.Threading.Tasks"
            ];
            foreach (var option in this._options)
            {
                //namespaces.Add(option.Parameter.Type.ContainingNamespace.ToDisplayString());
                if (option.IsArrayLike && option.ElementType is not null)
                {
                    namespaces.Add("System.Linq");
                    namespaces.Add(option.ElementType.ContainingNamespace.ToDisplayString());
                }
            }
            return  $$"""
                // auto-generated by SampleGenerator
                #nullable enable
                {{string.Join("\n", namespaces.Select(n => $"using {n};"))}}

                namespace {{this._namespace}}
                {
                {{this._programSource}}

                {{this._parserSource}}

                {{this._optionsSource}}
                }
                """;
        }
    }
}
