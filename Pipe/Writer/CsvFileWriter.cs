using System;
using System.IO;
using Importer.Pipe.Configuration;

namespace Importer.Pipe.Writer
{
    public class CsvFileWriter:IDisposable
    {
        public CsvFileWriter(Stream target, CsvFileConfiguration config)
        {
            this.config = config;
        }

        public Write()

        private CsvFileConfiguration config;
        private StreamWriter writer;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.writer?.Dispose();
                this.writer = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}