﻿namespace ConsoleAppDragonFruit
{
    internal class Program
    {
        /// <summary>
        /// Say hello
        /// </summary>
        /// <param name="aNumber">Leading number</param>
        /// <param name="ints">Some numbers</param>
        /// <param name="name">Their "name"</param>
        //public static void Main(SampleEnum2? notRequired, List<int> ints, string name = "bob")
        public static async Task<int> Main(int aNumber, FileInfo ints, string name = "george")
        {
            HelloFrom(aNumber + name);
            return 0;
        }


        static void HelloFrom(string name)
        {
            Task.Delay(1000).Wait();
            Console.WriteLine($"Hello from '{name}'");
        }
    }
}
