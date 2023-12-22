using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp8
{
    public class ArgumentInfo
    {
        public string Name { get; }
        public object? Value { get; }

        public ArgumentInfo(string name, object? value)
        {
            Name = name;
            Value = value;
        }
    }
}
