using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Readers
{
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class BufferedTextReader
    {
        public void SetDataSource(Stream source)
        {
            if (this.readerTask != null)
            {
                this.isChanging = true;
                this.readerTask.Wait();
            }

            this.dataSource = source;
            this.eof = false;
            this.readerTask = Task.Run(() => this.ReadInternal());
        }

        protected string GetNextLine()
        {
            if (this.buffer.TryDequeue(out string line))
            {
                return line;
            }
            else
            {
                while (!this.eof)
                {
                    if (this.buffer.TryDequeue(out line))
                    {
                        return line;
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
            }

            return null;
        }

        private void ReadInternal()
        {
            var sr = new StreamReader(this.dataSource);
            this.TotalBytes = sr.BaseStream.Length;
            while (!sr.EndOfStream)
            {
                if (this.buffer.Count < BUFFER_SIZE)
                {
                    var line = sr.ReadLine();
                    this.buffer.Enqueue(line);
                    this.LoadedBytes = sr.BaseStream.Position;
                }
                else
                {
                    Thread.Sleep(50);
                }
            }

            this.eof = true;
        }

        public long LoadedBytes { get; private set; }

        public long TotalBytes { get; private set; }


        public bool Eof => this.eof && this.buffer.Count == 0;

        private ConcurrentQueue<string> buffer=new ConcurrentQueue<string>();
        private Stream dataSource;

        private Task readerTask;

        private volatile bool isChanging = false;

        private volatile bool eof = false;

        private const int BUFFER_SIZE = 10000;
    }
}
