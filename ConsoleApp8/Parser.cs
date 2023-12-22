using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp8
{
#pragma warning disable CS8618
    public class Options
    {
        // use unique name so it won't collide with a real argument
        public bool ___ShowHelp___ { get; set; }

        public string arg1 { get; set; }
        public bool _setArg1 { get; set; }

        public int arg2 { get; set; }
        public bool _setArg2 { get; set; }

        // bools are special and don't have a set flag
        public bool arg3 { get; set; }

        public int argName { get; set; }
        public bool _setArgName { get; set; }

        // arguments with default values don't need a set flag
        public string argWithDefault { get; set; } = "default";

        // optional arguments don't need a set flag
        public string? argOptional { get; set; }

        public List<int> _argArrayBackingList = new();
        public bool _setArgArray { get; set; }
        public int[] argArray
        {
            get
            {
                return _argArrayBackingList.ToArray();
            }
        }

        public SampleEnum? enumOption { get; set; }

    }
#pragma warning restore CS8618

    public class Parser
    {
        private static readonly Dictionary<string, SampleEnum> _enumMapForSampleEnum = new(StringComparer.OrdinalIgnoreCase)
        {
            { "value1", SampleEnum.Value1 },
            { "value2", SampleEnum.Value2 },
            { "value3", SampleEnum.Value3 },
        };

        public Options Parse(string[] args)
        {
            Options arguments = new();
            for (int i = 0; i < args.Length && !arguments.___ShowHelp___; i++)
            {
                string arg = args[i];

                // grab the value for the option
                i++;

                switch (arg)
                {
                    case "--arg1":
                        arguments.arg1 = args[i];
                        arguments._setArg1 = true;
                        break;
                    case "--arg2":
                        arguments.arg2 = int.Parse(args[i]);
                        arguments._setArg2 = true;
                        break;
                    case "--arg3":
                        arguments.arg3 = true;
                        break;
                    case "--arg-name":
                        arguments.argName = int.Parse(args[i]);
                        arguments._setArgName = true;
                        break;
                    case "--arg-with-default":
                        arguments.argWithDefault = args[i];
                        break;
                    case "--arg-optional":
                        arguments.argOptional = args[i];
                        break;
                    case "--arg-array":
                        for (; i < args.Length; i++)
                        {
                            arg = args[i];
                            if (arg.StartsWith("-") && (arg.StartsWith("--") || arg == "-h" || arg == "-?"))
                            {
                                // reprocess this argument
                                i--;
                                break;
                            }
                            else
                            {
                                // add to the array
                                arguments._argArrayBackingList.Add(int.Parse(args[i]));
                            }
                        }

                        arguments._setArgArray = true;
                        break;
                    case "--enum-option":
                        if (!_enumMapForSampleEnum.ContainsKey(args[i]))
                        {
                            throw new ArgumentException($"'{args[i]}' is not a valid value for --enum-option");
                        }

                        arguments.enumOption = _enumMapForSampleEnum[args[i]];
                        break;
                    case "-h":
                    case "-?":
                    case "--help":
                        arguments.___ShowHelp___ = true;
                        break;
                    default:
                        throw new ArgumentException($"'{arg}' is not a recognized option");
                }
            }

            // verify required arguments unless help was requested
            if (!arguments.___ShowHelp___ && !(arguments._setArg1 && arguments._setArg2 && arguments._setArgName))
            {
                // missing some required arguments, build up a list of missing arguments
                List<string> missing = new();
                if (!arguments._setArg1)
                {
                    missing.Add("--arg1");
                }
                if (!arguments._setArg2)
                {
                    missing.Add("--arg2");
                }
                if (!arguments._setArgName)
                {
                    missing.Add("--arg-name");
                }

                throw new ArgumentException($"Missing required argument{(missing.Count > 1 ? "s" : "")}: {string.Join(", ", missing)}");
            }

            return arguments;
        }
    }
}
