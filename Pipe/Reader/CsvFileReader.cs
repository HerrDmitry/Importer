using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Importer.Pipe.Reader
{
    public class CsvFileReader : IFileReader
    {
        public CsvFileReader(Stream source, string qualifier = "\"", string delimiter = ",", int bufferSize = 1000000)
        {
            this.reader=new StreamReader(source);
            this.qualifier = !string.IsNullOrWhiteSpace(qualifier)?qualifier[0]:'"';
            this.delimiter = !string.IsNullOrWhiteSpace(delimiter)?delimiter[0]:',';
            this.qualifierStr = this.qualifier.ToString();
            this.qualifierStrDbl = this.qualifierStr + this.qualifierStr;
            this.delimiterStr = this.delimiter.ToString();
            this.bufferSize = bufferSize;
            this.buffer=new ConcurrentQueue<IEnumerable<string>>();
        }

        public IEnumerable<IEnumerable<string>> ReadData()
        {
            this.token=new CancellationTokenSource();
            this.readTask = Task.Run(() => this.FastReadLineTask(this.token.Token));

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

        private unsafe void FastReadLineTask(CancellationToken token)
        {
            var cBuff =new char[MAX_BUFFER_SIZE];
            int bufferLength = 0;
            int bufferPosition = 0;
            fixed (char* bPtr = cBuff)
            {
                long counter = 0;
                bufferLength = this.reader.ReadBlock(cBuff, 0, MAX_BUFFER_SIZE);
                bufferPosition = 0;
                var stopwatch=new Stopwatch();
                stopwatch.Start();
                var lastelapsed=0d;
                while (!this.reader.EndOfStream && !token.IsCancellationRequested)
                {
                    bool hasLine = false;
                    int qualifierCount = 0;
                    char[] line = null;
                    var startPosition = bufferPosition;

                    void appendToLine()
                    {
                        var lineLength = line?.Length > 0 ? line.Length : 0;
                        if (MAX_LINE_SIZE < lineLength + bufferPosition - startPosition)
                        {
                            throw new FileLoadException("Line is too big");
                        }
                        var newLine = new char[lineLength + bufferPosition - startPosition];
                        if (lineLength > 0)
                        {
                            Array.Copy(line, newLine, lineLength);
                        }
                        Array.Copy(cBuff, startPosition, newLine, lineLength, bufferPosition - startPosition);
                        line = newLine;
                    }

                    while (!hasLine)
                    {
                        while (bufferPosition < bufferLength && ((qualifierCount > 0 && qualifierCount % 2 != 0) || (bPtr[bufferPosition] != '\r' && bPtr[bufferPosition] != '\n')))
                        {
                            if (bPtr[bufferPosition] == this.qualifier) qualifierCount++;
                            bufferPosition++;
                        }

                        if (bufferPosition == bufferLength)
                        {
                            appendToLine();
                            startPosition = 0;
                            bufferPosition = 0;
                            bufferLength = this.reader.ReadBlock(cBuff, 0, MAX_BUFFER_SIZE);
                        }
                        else
                        {
                            if (bufferPosition > startPosition + 1)
                            {
                                appendToLine();
                                hasLine = true;
                            }
                            bufferPosition++;
                            startPosition = bufferPosition;
                        }
                    }

                    counter++;
                    this.SplitIntoColumns(line);
                    line = null;
                    if (stopwatch.Elapsed.TotalSeconds-lastelapsed >= 10)
                    {
                        Logger.GetLogger().DebugAsync($"Loaded {counter} lines in {stopwatch.Elapsed.TotalSeconds} seconds");
                        lastelapsed = stopwatch.Elapsed.TotalSeconds;
                    }
                }

                Logger.GetLogger().DebugAsync($"Loaded {counter} lines");
                this.eof = true;
            }
        }

        private unsafe IEnumerable<string> SplitIntoColumns(char[] source)
        {
            var result = new List<string>();
            if (source == null || source.Length == 0)
            {
                return result;
            }
            fixed (char* line = source)
            {
                var index = 0;
                while (index < source.Length)
                {
                    var expected = this.delimiter;
                    if (line[index] == this.delimiter)
                    {
                        index++;
                        result.Add(null);
                        continue;
                    }
                    int start, end;
                    if (line[index] == this.qualifier)
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
                        var idx = index;
                        while (idx < source.Length && line[idx] != expected) idx++;

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
                    
                    result.Add(new string(line, start, end).Replace(this.qualifierStrDbl, this.qualifierStr));
                }
            }
            return new List<string>();
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

                    var columns = this.Split(sourceLine.ToString());
                    sourceLine.Clear();
                    while (!token.IsCancellationRequested && this.bufferSize != 0 && this.buffer.Count > this.bufferSize)
                    {
                        Thread.Sleep(50);
                    }

                    this.buffer.Enqueue(columns);
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
                result.Add(source.Substring(start, end).Replace(this.qualifierStrDbl, this.qualifierStr));
            }
            return result;
        }


        private StreamReader reader;
        private ConcurrentQueue<IEnumerable<string>> buffer;
        private char qualifier;
        private string qualifierStr;
        private string qualifierStrDbl;
        private char delimiter;
        private string delimiterStr;
        private int bufferSize;
        private Task readTask;
        private CancellationTokenSource token;
        private volatile bool eof = false;

        private const int MAX_BUFFER_SIZE = 65535;
        private const int MAX_LINE_SIZE = 1024 * 1024;
    }
}
