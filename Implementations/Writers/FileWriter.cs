using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Importer.Interfaces;

namespace Importer.Writers
{
    public class FileWriter
    {
        public async Task FlushAsync()
        {
            this.isFlushing = true;
            while (taskCounter > 0)
            {
                Thread.Sleep(50);
            }
            await this.WriteoutAsync();
            lock (this.writeLocker)
            {
                this.writer.Flush();
            }
        }

        public void SetDataDestination(Stream stream)
        {
            this.writer = new StreamWriter(stream,Encoding.UTF8,10*1024*1024);
        }

        public void Close()
        {
            this.FlushAsync().Wait();
        }

        protected async Task WriteInternalAsync(StringBuilder s)
        {
            this.taskCounter++;
            await Task.Run(() =>
            {
                try
                {
                    lock (this.queueLocker)
                    {
                        this.queue.Append(s);
                    }

                    this.WriteoutAsync();
                }
                finally
                {
                    this.taskCounter--;
                }
            });
        }

        private async Task WriteoutAsync()
        {
            await Task.Run(() =>
            {
                string readyToWrite = null;
                lock (this.queueLocker)
                {
                    if (this.queue.Length >= MAX_BUFFER_LENGTH || this.isFlushing)
                    {
                        readyToWrite = this.queue.ToString();
                        this.queue.Clear();
                    }
                }
                if (readyToWrite != null)
                {
                    lock (this.writeLocker)
                    {
                        this.writer.Write(readyToWrite);
                    }
                }
            });
        }

        private StringBuilder queue = new StringBuilder();
        private object queueLocker=new object();
        private object writeLocker=new object();
        private TextWriter writer;
        private volatile int taskCounter=0;
        private const int MAX_BUFFER_LENGTH = 1024*1024;
        private volatile bool isFlushing=false;
    }
}
