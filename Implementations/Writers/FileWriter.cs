using System;
using System.Collections.Concurrent;
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
            await Task.Run(() =>
            {
                if (this.isClosing)
                {
                    return;
                }
                this.isFlushing = true;
                while (this.isFlushing)
                {
                    Thread.Sleep(1);
                }
            });
        }

        public void SetDataDestination(Stream stream)
        {
            this.writer = new StreamWriter(stream);
            this.writerTask = Task.Run(() => this.WriteoutTask());
        }

        public void Close()
        {
            this.isClosing = true;
            this.writerTask.Wait();
        }

        protected void WriteInternal(StringBuilder s)
        {
            if (this.isClosing)
            {
                throw new ObjectDisposedException("Output stream is done for");
            }
            this.queue.Enqueue(s);
        }

        private void WriteoutTask()
        {
            try
            {
                Logger.GetLogger().DebugAsync("Starting file writer's thread");
                while (!this.isClosing || this.queue.Count>0)
                {
                    while (this.queue.TryDequeue(out StringBuilder s))
                    {
                        this.writer.Write(s);
                    }
                    
                    if (this.queue.Count==0)
                    {
                        Thread.Sleep(50);
                    }

                    if (this.isFlushing || this.isClosing)
                    {
                        Logger.GetLogger().DebugAsync("Flushing the file buffers");
                        this.writer.Flush();
                        this.isFlushing = false;
                    }
                }

                Logger.GetLogger().DebugAsync("Disposing the file writer");
                this.writer.Dispose();
            }
            catch (Exception ex)
            {
                Logger.GetLogger().ErrorAsync(ex.Message);
            }
            finally
            {
                Logger.GetLogger().DebugAsync("Exiting writer's thread");
            }
        }

        private Task writerTask = null;
        private ConcurrentQueue<StringBuilder> queue = new ConcurrentQueue<StringBuilder>();
        //private StringBuilder queue = new StringBuilder();
//        private object queueLocker=new object();
//        private object writeLocker=new object();
        private TextWriter writer;
//        private volatile int taskCounter=0;
//        private const int MAX_BUFFER_LENGTH = 1024*1024;
        private volatile bool isFlushing=false;

        private volatile bool isClosing = false;
    }
}
