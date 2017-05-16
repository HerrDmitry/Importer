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
            this.dictBuffer=new ConcurrentBag<Dictionary<string,IValue>>();
            this.buffer=new ConcurrentBag<string>();
            this.tokenSource=new CancellationTokenSource();
            var tasks = new List<Task>();
            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                tasks.Add(Task.Run(() => this.PrepareTask(tokenSource.Token)));
            }
            tasks.Add(Task.Run(()=>this.WriteInternalTask(tokenSource.Token)));
            this.tasks = tasks.ToArray();
        }

        public void Write(Dictionary<string,IValue> values)
        {
            this.WriteLine(values);
        }

        public void WriteLine(Dictionary<string,IValue> values)
        {
            while (this.dictBuffer.Count > MAX_BUFFER_SIZE)
            {
                Logger.GetLogger().DebugAsync("Reached writer buffer limit");
                Thread.Sleep(50);
            }

            this.dictBuffer.Add(values);
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
                Task.WaitAll(this.tasks);
                this.writer.Flush();
                this.writer?.Dispose();
                this.writer = null;
            }
        }

        private void PrepareTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested || this.dictBuffer.Count>0)
            {
                if (this.dictBuffer.TryTake(out Dictionary<string, IValue> values))
                {
                    var strings = this.config.Columns.Select(x => values.TryGetValue(x.Source, out IValue value) ? value.ToString(x.Format) : "").ToList();
                    this.PrepareRecord(strings);
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }

        private unsafe void PrepareRecord(List<string> values)
        {
            var qualifier = this.config.TextQualifierChar;
            var sb=Memory.GetAvailableStringBuilder();
            for (var i = 0; i < values.Count; i++)
            {
                if (i > 0) sb.Append(this.config.Delimiter);

                var value = values[i];

                var lineSource = Memory.GetAvailableCharArray(value.Length);
                value.CopyTo(0,lineSource,0,value.Length);
                var needQualifier = false;
                var line = Memory.GetAvailableCharArray(value.Length * 2);
                fixed (char* linePtr = line, lineSourcePtr = lineSource)
                {
                    var ti = 0;
                    for (var si = 0; si < value.Length; si++)
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
                        sb.Append(this.config.TextQualifier);
                    }
                    sb.Append(linePtr,ti);
                    if (needQualifier)
                    {
                        sb.Append(this.config.TextQualifier);
                    }
                }

                Memory.StoreArray(lineSource);
                Memory.StoreArray(line);
            }

            sb.AppendLine();
            if (this.buffer.Count > MAX_BUFFER_SIZE)
            {
                Logger.GetLogger().DebugAsync("File stream buffer reached limit");
                Thread.Sleep(50);
            }
            this.buffer.Add(sb.ToString());
            Memory.StoreStringBuilder(sb);
        }

        private void WriteInternalTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested || this.buffer.Count>0)
            {
                if (this.buffer.TryTake(out string line))
                {
                    this.writer.Write(line);
                }
                else
                {
                    Thread.Sleep(50);
                }

            }
        }

        private CsvFileConfiguration config;
        private StreamWriter writer;

        private ConcurrentBag<string> buffer;
        private ConcurrentBag<Dictionary<string, IValue>> dictBuffer;

        private const int MAX_BUFFER_SIZE = 10000;
        private CancellationTokenSource tokenSource;
        private Task[] tasks;
    }
}