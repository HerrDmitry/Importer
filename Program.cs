using System;
using System.IO;
using System.Linq;
using Importer.Implementations;

namespace Importer
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var configPath = args.FirstOrDefault(x => x.ToUpper().StartsWith("/C:"))?.Substring(3);
            if (string.IsNullOrEmpty(configPath))
            {
                PrintUsage();
                return -1;
            }

            var config=new Configuration(configPath);
            var reader = config.GetReaders().First().Value;
            var data = reader.ReadFromStream(File.OpenRead("C:\\Users\\Dmitry\\Source\\apps.csv"))
                .Select(x => new {Values = x.GetValues().ToList()}).ToList();

            return 0;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.Write("/c:{configuration file path} ");
            Console.WriteLine("[/f:{TableName}:{source file name} ...]");
        }
    }
}
