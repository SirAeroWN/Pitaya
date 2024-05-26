using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp8
{
    class Program
    {
        /// <summary>
        /// My sample program
        /// </summary>
        /// <param name="required"></param>
        /// <param name="optionalFile"></param>
        /// <param name="hasADefault"></param>
        public static void Main(int required, FileInfo? optionalFile, string hasADefault = "My great default")
        {
            Console.WriteLine($"required: {required}");
            Console.WriteLine($"optionalFile was {(optionalFile == null ? "not passed" : "passed")}");
            Console.WriteLine($"hasADefault: {hasADefault}");
        }


        static void HelloFrom(string name)
        {
            Task.Delay(1000).Wait();
            Console.WriteLine($"Hello from '{name}'");
        }
    }

    enum SampleEnum2
    {
        Value1,
        Value2,
        Value3
    }
}
