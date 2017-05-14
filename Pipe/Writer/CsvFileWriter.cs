using System.IO;
using Importer.Pipe.Configuration;

namespace Importer.Pipe.Writer
{
    public class CsvFileWriter
    {
        public CsvFileWriter(Stream target, CsvFileConfiguration config)
        {
            this.config = config;
        }

        private CsvFileConfiguration config;
    }
}