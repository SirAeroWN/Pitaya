using CLIParserSourceGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLIParserSourceGenerator
{
    internal class ParameterDescriptionList : IEnumerable<ParameterDescription>
    {
        List<ParameterDescription> parameterDescriptions { get; }

        public ParameterDescriptionList(IEnumerable<ParameterDescription> parameterDescriptions)
        {
            this.parameterDescriptions = new List<ParameterDescription>(parameterDescriptions);
        }

        public static ParameterDescriptionList Create(List<OptionInfo> options, List<CommentInfo> comments)
        {
            List<ParameterDescription> parameterDescriptions = new List<ParameterDescription>(options.Count);

            // track the options we've already added
            HashSet<string> addedOptions = new HashSet<string>();
            // first, iterate over the comments and add the options that have comments
            foreach (CommentInfo comment in comments)
            {
                OptionInfo? option = options.FirstOrDefault(o => o.OptionName == comment.OptionName);
                if (option != null)
                {
                    parameterDescriptions.Add(new ParameterDescription(option, comment));
                    addedOptions.Add(option.OptionName);
                }
            }

            // now add any remaining options that don't have comments
            foreach (OptionInfo option in options)
            {
                if (!addedOptions.Contains(option.OptionName))
                {
                    parameterDescriptions.Add(new ParameterDescription(option, null));
                }
            }

            return new ParameterDescriptionList(parameterDescriptions);
        }

        public IEnumerator<ParameterDescription> GetEnumerator()
        {
            return this.parameterDescriptions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
