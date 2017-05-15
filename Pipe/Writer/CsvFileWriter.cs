using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var data = (from c in this.config.Columns
                join v in values on c.Name equals v.Column.Name
                select v.ToString(c.Format)).ToList();
            for (var i = 0; i < data.Count; i++)
            {
                if(i>0) this.writer.Write(this.config.Delimiter);
                var value = data[i];
                if (value.IndexOf(this.config.TextQualifier) >= 0)
                {
                    this.writer.Write(this.config.TextQualifier);
                    this.writer.Write(value.Replace(this.config.TextQualifier, this.config.TextQualifier + this.config.TextQualifier));
                    this.writer.Write(this.config.TextQualifier);
                }
                else
                {
                    this.writer.Write(value);
                }
            }
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