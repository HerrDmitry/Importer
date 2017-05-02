using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Importer
{
    public class Logger
    {
        private static Logger instance=new Logger();

        private Logger()
        {
        }

        public static Logger GetLogger()
        {
            return instance;
        }

        public void ErrorAsync(string message)
        {
            messageQueue.Enqueue(new MessageItem {Message = $"Error - {message}", Color = ConsoleColor.Red });
            this.Start();
        }

        public void InfoAsync(string message)
        {
            if (this.loggingLevel == LogLevel.Info || this.loggingLevel == LogLevel.Debug)
            {
                messageQueue.Enqueue(new MessageItem {Message = $"Info - {message}", Color = ConsoleColor.Green});
                this.Start();
            }
        }

        public void DebugAsync(string message)
        {
            if (this.loggingLevel == LogLevel.Debug)
            {
                messageQueue.Enqueue(new MessageItem {Message = $"Debug - {message}", Color = ConsoleColor.White});
                this.Start();
            }
        }

        public void MessageAsync(string message)
        {
            messageQueue.Enqueue(new MessageItem { Message = message, Color = ConsoleColor.Gray });
            this.Start();
        }

        public void SetLogginLevel(LogLevel level)
        {
            this.loggingLevel = level;
        }

        public void Flush()
        {
            while (!this.messageQueue.IsEmpty)
            {
                Thread.Sleep(50);
            }
        }

        private void Start()
        {
            lock (instance)
            {
                if (this.queueTask != null)
                {
                    return;
                }

                this.queueTask=new Task(() =>
                {
                    while (!this.messageQueue.IsEmpty)
                    {
                        if (this.messageQueue.TryDequeue(out MessageItem message))
                        {
                            this.WriteMessage(message);
                        }
                    }

                    lock (instance)
                    {
                        this.queueTask = null;
                    }
                });

                this.queueTask.Start();
            }
        }

        private Task queueTask = null;

        private ConcurrentQueue<MessageItem> messageQueue=new ConcurrentQueue<MessageItem>();

        private void WriteMessage(MessageItem message)
        {
            lock (instance)
            {
                Debug.WriteLine(message.Message);
                var color = Console.ForegroundColor;
                Console.ForegroundColor = message.Color;
                Console.WriteLine(message.Message);
                Console.ForegroundColor = color;
            }
        }

        private LogLevel loggingLevel=LogLevel.Error;

        public struct MessageItem
        {
            public string Message;
            public ConsoleColor Color;
        }
        public enum LogLevel
        {
            Debug, Info, Error
        }
    }
}
