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
    using Importer.Pipe.Configuration;
    using Importer.Pipe.Parsers;

    public class CsvFileReader : IFileReader
    {
        public CsvFileReader(Stream source, CsvFileConfiguration config, int bufferSize = 1000000)
        {
            this.reader = new StreamReader(source);
            this.qualifier = !string.IsNullOrWhiteSpace(config?.TextQualifier) ? config.TextQualifier[0] : '"';
            this.delimiter = !string.IsNullOrWhiteSpace(config?.Delimiter) ? config.Delimiter[0] : ',';
            this.bufferSize = bufferSize;
            this.buffer = new ConcurrentQueue<IEnumerable<string>>();
            this.config = config;
        }

        public IEnumerable<IEnumerable<IValue>> ReadData()
        {
            this.token=new CancellationTokenSource();
            this.readTask = Task.Run(() => this.FastReadLineTask(this.token.Token));

            var recordParser = new RecordParser(this.config.Columns);

            while (!this.eof || this.buffer.Count > 0)
            {
                if (this.buffer.TryDequeue(out IEnumerable<string> record))
                {
                    yield return recordParser.Parse(record);
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
            fixed (char* bPtr = cBuff)
            {
                long counter = 0;
                var bufferLength = this.reader.ReadBlock(cBuff, 0, MAX_BUFFER_SIZE);
                var bufferPosition = 0;
                var stopwatch=new Stopwatch();
                stopwatch.Start();
                var lastelapsed=0d;
                while ((!this.reader.EndOfStream || bufferPosition<bufferLength) && !token.IsCancellationRequested )
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
                            if (this.reader.EndOfStream)
                            {
                                break;
                            }
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
                    var columns = this.SplitIntoColumns(line);
                    while (!token.IsCancellationRequested && this.bufferSize != 0 && this.buffer.Count > this.bufferSize)
                    {
                        Thread.Sleep(50);
                    }

                    this.buffer.Enqueue(columns);
                    line = null;
                    if (stopwatch.Elapsed.TotalSeconds-lastelapsed >= 10)
                    {
                        Logger.GetLogger().DebugAsync($"Loaded {counter} lines in {stopwatch.Elapsed.TotalSeconds} seconds");
                        lastelapsed = stopwatch.Elapsed.TotalSeconds;
                    }
                }

                stopwatch.Stop();
                Logger.GetLogger().DebugAsync($"Loaded {counter} lines in {stopwatch.Elapsed.TotalSeconds} seconds.");
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
                    int start, len;
                    if (line[index] == this.qualifier)
                    {
                        expected = this.qualifier;
                        start = index + 1;
                    }
                    else
                    {
                        start = index;
                    }

                    len = 0;
                    var done = false;
                    while (!done)
                    {
                        index++;
                        var idx = index;
                        while (idx < source.Length && line[idx] != expected) idx++;

                        len = idx - start;
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
                    var resultLine = new char[len];
                    fixed (char* resultPtr = resultLine)
                    {
                        var idx = 0;
                        var end = start + len;
                        for (var i = start; i < end; i++)
                        {
                            if (line[i] == this.qualifier)
                            {
                                i++;
                                if (i < end)
                                {
                                    resultPtr[idx++] = line[i];
                                }
                            }
                            else
                            {
                                resultPtr[idx++] = line[i];
                            }
                        }
                        result.Add(new string(resultPtr, 0, idx));
                    }
                }
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

        private CsvFileConfiguration config;

        private const int MAX_BUFFER_SIZE = 1024*1024;
        private const int MAX_LINE_SIZE = 1024 * 1024*20;
    }
}
