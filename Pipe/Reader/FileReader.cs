using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Importer.Pipe.Configuration;

namespace Importer.Pipe.Reader
{
    public static class FileReader
    {
        public static IFileReader GetFileReader(Stream source,FileConfiguration config)
        {
            switch (config.Type)
            {
                case "CSV":
                    var csvConfig = config as CsvFileConfiguration;
                    return new CsvFileReader(source,csvConfig?.TextQualifier, csvConfig?.Delimiter);
                default:
                    throw new ArgumentOutOfRangeException($"File type \"{config.Type}\" is not supported.");
            }
        }
    }
}
