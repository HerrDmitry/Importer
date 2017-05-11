using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Importer.Pipe.Reader
{
    public class CsvFileReader : IFileReader
    {
        public CsvFileReader(Stream source, string qualifier = "\"", string delimiter = ",", int bufferSize = 1000)
        {
            this.reader=new StreamReader(source);
            this.qualifier = !string.IsNullOrWhiteSpace(qualifier)?qualifier[0]:'"';
            this.delimiter = !string.IsNullOrWhiteSpace(delimiter)?delimiter[0]:',';
            this.bufferSize = bufferSize;
            this.buffer=new ConcurrentQueue<IEnumerable<string>>();
        }

        public IEnumerable<IEnumerable<string>> ReadData()
        {
            this.token=new CancellationTokenSource();
            this.readTask = Task.Run(() => this.ReadLinesTask(this.token.Token));

            while (!this.eof || this.buffer.Count > 0)
            {
                if (this.buffer.TryDequeue(out IEnumerable<string> record))
                {
                    yield return record;
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.readTask != null && !this.eof)
                {
                    this.token.Cancel();
                    this.readTask.Wait();
                }
                if (this.reader != null)
                {
                    this.reader.Dispose();
                    this.reader = null;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ReadLinesTask(CancellationToken token)
        {
            long counter = 0;
            var qualifierCount = 0;

            var sourceLine = new StringBuilder();
            while (!this.reader.EndOfStream && !token.IsCancellationRequested)
            {
                var line = this.reader.ReadLine();
                if (sourceLine.Length > 0)
                {
                    sourceLine.AppendLine();
                }
                sourceLine.Append(line);
                var qualifierIndex = line.IndexOf(this.qualifier);
                while (qualifierIndex >= 0)
                {
                    qualifierCount++;
                    qualifierIndex++;
                    if (qualifierIndex >= line.Length)
                    {
                        break;
                    }

                    qualifierIndex = line.IndexOf(this.qualifier, qualifierIndex);
                }

                if (qualifierCount == 0 || qualifierCount % 2 == 0)
                {

                    while (!token.IsCancellationRequested && this.bufferSize != 0 && this.buffer.Count > this.bufferSize)
                    {
                        Thread.Sleep(50);
                    }

                    this.buffer.Enqueue(this.Split(sourceLine.ToString()));
                    sourceLine.Clear();
                    counter++;

                }

            }

            Logger.GetLogger().DebugAsync($"Loaded {counter} records.");
            this.eof = true;
        }

        private IEnumerable<string> Split(string source)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(source))
            {
                return result;
            }
            var index = 0;
            while (index < source.Length)
            {
                var expected = this.delimiter;
                if (source[index] == this.delimiter)
                {
                    index++;
                    result.Add(null);
                    continue;
                }
                int start, end;
                if (source[index] == this.qualifier)
                {
                    expected = this.qualifier;
                    start = index + 1;
                }
                else
                {
                    start = index;
                }

                end = start;
                var done = false;
                while (!done)
                {
                    index++;
                    var idx = source.IndexOf(expected, index);
                    if (idx < 0)
                    {
                        idx = source.Length;
                    }
                    end = idx - start;
                    index = idx;

                    if (index < source.Length)
                    {
                        if (index < source.Length - 1 && source[index] == this.qualifier)
                        {
                            if (source[index + 1] == this.qualifier)
                            {
                                index++;
                                continue;
                            }
                            else if (source[index + 1] == this.delimiter)
                            {
                                index++;
                                index++;
                                done = true;
                            }
                            else
                            {
                                throw new FormatException("Row in incorrect format");
                            }
                        }
                        else
                        {
                            done = true;
                            index++;
                        }
                    }
                    else
                    {
                        done = true;
                    }
                }
                result.Add(source.Substring(start, end).Replace(this.qualifier.ToString(), ""));
            }
            return result;
        }


        private StreamReader reader;
        private ConcurrentQueue<IEnumerable<string>> buffer;
        private char qualifier;
        private char delimiter;
        private int bufferSize;
        private Task readTask;
        private CancellationTokenSource token;
        private volatile bool eof = false;
    }
}
