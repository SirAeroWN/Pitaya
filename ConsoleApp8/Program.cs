using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp8
{
    class Program
    {
        public static void Main(MyNegativeInt negative)
        {
            Console.WriteLine(negative);
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

    class MyNegativeInt
    {
        public int Value { get; private set; }

        private MyNegativeInt(int value)
        {
            this.Value = value;
        }

        public static MyNegativeInt Parse(string value)
        {
            int valueAsInt = int.Parse(value);

            if (valueAsInt >= 0)
            {
                throw new ArgumentException($"{valueAsInt} must be negative");
            }

            return new MyNegativeInt(valueAsInt);
        }

        public override string ToString()
        {
            return $"MyNegativeInt has a value of {this.Value}";
        }
    }
}
