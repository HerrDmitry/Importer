using System;
using System.IO;
using System.Linq;
using Importer.Implementations;

namespace Importer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var config=new Configuration("Configurations\\TestExport.json");
            var reader = config.GetReaders().First().Value;
            var data = reader.ReadFromStream(File.OpenRead("C:\\Users\\Dmitry\\Source\\export.csv"))
                .Select(x => new {Values = x.GetValues().ToList()}).ToList();
        }
    }
}
