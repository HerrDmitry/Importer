using System;
using System.IO;
using Importer.Pipe.Configuration;

namespace Importer.Pipe.Writer
{
    public static class FileWriter
    {
        public static IFileWriter GetFileWriter(Stream source,FileConfiguration config)
        {
            switch (config.Type)
            {
                case "CSV":
                    var csvConfig = config as CsvFileConfiguration;
                    return new CsvFileWriter(source,csvConfig);
                default:
                    throw new ArgumentOutOfRangeException($"File type \"{config.Type}\" is not supported.");
            }
        }
    }
}