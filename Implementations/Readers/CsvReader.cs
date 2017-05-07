using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Importer.Records;
using Importer.Configuration;
using Importer.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Importer.Readers
{
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public class CsvReader : IReader
    {
        private Stream dataSource;

        protected CsvReader()
        {
        }

        public CsvReader(JObject configuration)
        {
            this.configuration = Configuration.Configuration.ParseConfiguration<CsvReaderConfiguration>(configuration);
        }

        public void SetDataSource(Stream source){
            this.dataSource = source;
            var task = Task.Run(() => this.ReadLinesTask());
        }

        protected IEnumerable<StringBuilder> ReadLines()
        {
            long counter = 0;
            this.eof = false;
            while (!this.eof || this.csvLines.Count>0)
            {
                if(this.csvLines.TryDequeue(out StringBuilder sourceLine)) {
                    counter++;
                    yield return sourceLine;
                }
            }
            Logger.GetLogger().DebugAsync($"Loaded {counter} records.");
        }

        public virtual IEnumerable<IRecord> ReadData()
        {
            return this.ReadLines().Select(sourceLine => Record<CsvRecord>.Factory.GetRecord(this.configuration, sourceLine));
        }

        private void ReadLinesTask()
        {
            var sr = new StreamReader(this.dataSource);
            this.TotalBytes = sr.BaseStream.Length;
            var qualifier = this.configuration.TextQualifierChar;
            var bufferSize = this.configuration.ReadBuffer.GetValueOrDefault(0) > 0 ? this.configuration.ReadBuffer.Value : LINES_BUFFER;
            while (!sr.EndOfStream)
            {
                var sourceLine = new StringBuilder();
                var qualifierCount = 0;
                while (qualifierCount == 0 || qualifierCount % 2 != 0)
                {
                    if (sourceLine.Length > 0)
                    {
                        sourceLine.AppendLine();
                    }
                    var line = sr.ReadLine();
                    this.LoadedBytes = sr.BaseStream.Position;
                    sourceLine.Append(line);

                    var index = line.IndexOf(qualifier);
                    while (index >= 0)
                    {
                        qualifierCount++;
                        index++;
                        if (index >= line.Length)
                        {
                            break;
                        }

                        index = line.IndexOf(qualifier, index);
                    }

                    if (qualifierCount == 0)
                    {
                        break;
                    }
                }

                if (this.csvLines.Count < bufferSize)
                {
                    this.csvLines.Enqueue(sourceLine);
                }
                else
                {
                    Thread.Sleep(50);
                }

                this.eof = true;
            }
        }

        public List<ColumnInfo> Columns => new List<ColumnInfo>(this.configuration.Columns);

        public long LoadedBytes { get; private set; }

        public long TotalBytes { get; private set; }

        protected CsvReaderConfiguration configuration;

        private volatile bool eof = false;

        private const int LINES_BUFFER = 10000;

        private ConcurrentQueue<StringBuilder> csvLines=new ConcurrentQueue<StringBuilder>();
        public class CsvReaderConfiguration:CsvFileConfiguration<ColumnInfo>{}
    }
}
