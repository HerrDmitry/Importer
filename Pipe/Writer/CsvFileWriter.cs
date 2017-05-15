using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            this.buffer=new ConcurrentBag<IEnumerable<IValue>>();
            this.tokenSource=new CancellationTokenSource();
            this.writeTask = Task.Run(() => this.WriteTask(tokenSource.Token));
        }

        public void Write(IEnumerable<IValue> values)
        {
            this.WriteLine(values);
        }

        public void WriteLine(IEnumerable<IValue> values)
        {
            while (this.buffer.Count > MAX_BUFFER_SIZE)
            {
                Thread.Sleep(50);
            }

            this.buffer.Add(values);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.tokenSource.Cancel();
                this.writeTask.Wait();
                this.writer.Flush();
                this.writer?.Dispose();
                this.writer = null;
            }
        }

        private void WriteTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (this.buffer.TryTake(out IEnumerable<IValue> values))
                {
                    this.WriteInternal(values);
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }

        private void WriteInternal(IEnumerable<IValue> values)
        {
            for (var i = 0; i < config.Columns.Count; i++)
            {
                if (i > 0) this.writer.Write(this.config.Delimiter);
                var val = values.FirstOrDefault(x=>x.Column.Name==this.config.Columns[i].Source);

                if (val != null)
                {
                    var value = val.ToString();

                    if (value.IndexOf(this.config.TextQualifier) >= 0 || value.IndexOf('\r') >= 0 || value.IndexOf('\n') >= 0)
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

            }
            //this.writer.WriteLine();
        }

        private CsvFileConfiguration config;
        private StreamWriter writer;

        private ConcurrentBag<IEnumerable<IValue>> buffer;

        private const int MAX_BUFFER_SIZE = 10000;
        private CancellationTokenSource tokenSource;
        private Task writeTask;

    }
}