using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Importer.Interfaces;

namespace Importer.Writers
{
    public class FileWriter
    {
        public async Task FlushAsync()
        {
            await Task.Run(()=>{
                this.isFlushing = true;
                this.Writeout();
            });
        }

        public void SetDataDestination(Stream stream)
        {
            this.writer = new StreamWriter(stream);
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
                        if (this.queue == null)
                        {
                            this.queue=new StringBuilder();
                        }

                        this.queue.Append(s);
                    }

                    this.Writeout();
                }
                finally
                {
                    this.taskCounter--;
                }
            });
        }

        private void Writeout(){
            StringBuilder readyToWrite = null;
            lock (this.queueLocker)
            {
                if (this.queue?.Length >= MAX_BUFFER_LENGTH || this.isFlushing)
                {
                    readyToWrite = this.queue;
                    this.queue = null;
                }
            }
            if (readyToWrite != null)
            {
                lock (this.writeLocker)
                {
                    this.writer.WriteAsync(readyToWrite.ToString());
                }
            }
        }

        private StringBuilder queue = null;
        private object queueLocker=new object();
        private object writeLocker=new object();
        private TextWriter writer;
        private volatile int taskCounter=0;
        private const int MAX_BUFFER_LENGTH = 65535;
        private volatile bool isFlushing=false;
    }
}
