using System.Collections.Generic;
using Importer.Interfaces;

namespace Importer.Readers
{
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class BufferedTextReader:ITextReader
    {
        public BufferedTextReader(Stream source)
        {
            this.dataSource = source;
            this.eof = false;
            this.readerTask = Task.Run(() => this.ReadInternal());
        }

        public IEnumerable<string> GetLines()
        {
            while (!this.eof || this.buffer.Count>0)
            {
                if (this.buffer.TryDequeue(out string line))
                {
                    yield return line;
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
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

        private volatile bool eof = false;

        private const int BUFFER_SIZE = 10000;
    }
}
