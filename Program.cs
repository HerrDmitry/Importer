using System;
using Importer.Implementations;

namespace Importer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var config=new Configuration("Configurations\\TestExport.json");
        }
    }
}
