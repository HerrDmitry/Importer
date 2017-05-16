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
    using System.Text;

    public class CsvFileWriter:IDisposable,IFileWriter
    {
        public CsvFileWriter(Stream target, CsvFileConfiguration config)
        {
            this.config = config;
            this.writer=new StreamWriter(target);
            this.buffer=new ConcurrentBag<List<string>>();
            this.tokenSource=new CancellationTokenSource();
            this.writeTask = Task.Run(() => this.WriteTask(tokenSource.Token));
        }

        public void Write(Dictionary<string,IValue> values)
        {
            this.WriteLine(values);
        }

        public void WriteLine(Dictionary<string,IValue> values)
        {
            while (this.buffer.Count > MAX_BUFFER_SIZE)
            {
                Logger.GetLogger().DebugAsync("Reached writer buffer limit");
                Thread.Sleep(50);
            }

            var strings = this.config.Columns.Select(x => values.TryGetValue(x.Source, out IValue value) ? value.ToString(x.Format) : "").ToList();
            this.buffer.Add(strings);
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
                if (this.buffer.TryTake(out List<string> values))
                {
                    this.WriteInternal(values);
                }
                else
                {
                    Logger.GetLogger().DebugAsync("Writer buffer is empty");
                    Thread.Sleep(50);
                }
            }
        }

        private unsafe void WriteInternal(List<string> values)
        {
            var qualifier = this.config.TextQualifierChar;
            for (var i = 0; i < values.Count; i++)
            {
                if (i > 0) this.writer.Write(this.config.Delimiter);

                var value = values[i];

                var lineSource = new char[value.Length];
                value.CopyTo(0,lineSource,0,value.Length);
                var needQualifier = false;
                var line = new char[value.Length * 2];
                fixed (char* linePtr = line, lineSourcePtr = lineSource)
                {
                    var ti = 0;
                    for (var si = 0; si < lineSource.Length; si++)
                    {
                        if (lineSourcePtr[si] == qualifier)
                        {
                            linePtr[ti++] = qualifier;
                            needQualifier = true;
                        }

                        if (!needQualifier && (lineSourcePtr[si] == '\r' || lineSourcePtr[si] == '\n'))
                        {
                            needQualifier = true;
                        }

                        linePtr[ti++] = lineSourcePtr[si];
                    }
                    if (needQualifier)
                    {
                        this.writer.Write(this.config.TextQualifier);
                    }
                    this.writer.Write(line,0,ti);
                    if (needQualifier)
                    {
                        this.writer.Write(this.config.TextQualifier);
                    }
                }

            }

            this.writer.WriteLine();
        }

        private CsvFileConfiguration config;
        private StreamWriter writer;

        private ConcurrentBag<List<string>> buffer;

        private const int MAX_BUFFER_SIZE = 10000;
        private CancellationTokenSource tokenSource;
        private Task writeTask;

    }
}