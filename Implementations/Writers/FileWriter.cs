using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Importer.Configuration;
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

        public void SetDataDestination(Stream stream, FileWriter errorOutputDestination=null)
        {
            this.writer = new StreamWriter(stream);
            this.writerTask = Task.Run(() => this.WriteoutTask());
            if (errorOutputDestination != null)
            {
                this.ErrorWriter = errorOutputDestination;
            }
        }

        public virtual void Close()
        {
            this.isClosing = true;
            this.writerTask.Wait();
            Logger.GetLogger().InfoAsync($"Processed successfully {this.recordCounter} records, had errors {this.exceptionCounter} records, total {this.recordCounter + this.exceptionCounter} records");
        }

        public virtual void Write(IRecord record)
        {
            try
            {
                var builder = this.ConvertRecord(record);
                if (builder != null)
                {
                    this.WriteInternal(builder);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().ErrorAsync(ex.Message);
            }
        }

        public async Task WriteAsync(IRecord record)
        {
            await Task.Run(() =>
            {
                this.Write(record);
            });
        }

        public virtual void Write(StringBuilder str)
        {
            this.WriteInternal(str);
        }

        protected virtual StringBuilder ConvertRecord(IRecord record)
        {
            return null;
        }

        protected virtual void HandleException(IParser column, IRecord record)
        {
            lock (this.writer)
            {
                this.exceptionCounter++;
                var message=new StringBuilder();
                message.Append($"Exception on row {record.RowNumber}, column {column.ColumnName}").AppendLine().Append("\t").Append(record.Source);
                this.ErrorWriter?.Write(message);
                if (this.ErrorWriter == null)
                {
                    this.ErrorWriter=new FileWriter();
                }
            }
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
                        this.recordCounter++;
                    }

                    Thread.Sleep(1);

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

        private FileWriter ErrorWriter;

        private Task writerTask = null;
        private ConcurrentQueue<StringBuilder> queue = new ConcurrentQueue<StringBuilder>();
        private TextWriter writer;
        private volatile bool isFlushing=false;
        private volatile bool isClosing = false;
        private long recordCounter = 0;
        private long exceptionCounter = 0;
    }
}
