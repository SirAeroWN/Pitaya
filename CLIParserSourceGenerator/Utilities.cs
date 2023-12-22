using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CLIParserSourceGenerator
{
    internal static class Utilities
    {
        public static string Optionify(string name)
        {
            var rgx = new Regex(@"[A-Z](?=[a-z])");
            return "--" + rgx.Replace(name, m => "-" + m.Value.ToLower()).ToLower();
        }

        public static string Propertyify(string name)
        {
            return name;
        }
    }
}
