using System;
using System.Collections.Generic;
using System.IO;
using Importer.Pipe.Configuration;
using Importer.Pipe.Parsers;

namespace Importer.Pipe.Writer
{
    public class CsvFileWriter:IDisposable,IFileWriter
    {
        public CsvFileWriter(Stream target, CsvFileConfiguration config)
        {
            this.config = config;
            this.writer=new StreamWriter(target);
        }

        public void Write(IEnumerable<IValue> values)
        {
            foreach (var value in values)
            {
                this.writer.Write(value);
            }
        }

        public void WriteLine(IEnumerable<IValue> values = null)
        {
            this.Write(values);
            this.writer.WriteLine();
        }


        private CsvFileConfiguration config;
        private StreamWriter writer;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.writer.Flush();
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