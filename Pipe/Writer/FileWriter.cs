using System;
using System.IO;
using Importer.Pipe.Configuration;

namespace Importer.Pipe.Writer
{
    public static class FileWriter
    {
        public static IFileWriter GetFileWriter(Stream target, FileConfiguration config)
        {
            switch (config.Type)
            {
                case "CSV":
                    var csvConfig = config as CsvFileConfiguration;
                    return new CsvFileWriter(target, csvConfig);
                case "CSVMultiline":
                    var csvMultilineConfig = config as CsvMultilineFileConfiguration;
                    return new CsvMultilineFileWriter(target, csvMultilineConfig);
                default:
                    throw new ArgumentOutOfRangeException($"File type \"{config.Type}\" is not supported.");
            }
        }
    }
}