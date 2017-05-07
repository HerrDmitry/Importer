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
        }

        protected IEnumerable<StringBuilder> ReadLines()
        {
            var sr = new StreamReader(this.dataSource);
            this.TotalBytes = sr.BaseStream.Length;
            long counter = 0;
            var qualifier = this.configuration.TextQualifierChar;
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

                counter++;
                yield return sourceLine;
            }

            Logger.GetLogger().DebugAsync($"Loaded {counter} records.");
        }

        public virtual IEnumerable<IRecord> ReadData()
        {
            return this.ReadLines().Select(sourceLine => Record<CsvRecord>.Factory.GetRecord(this.configuration, sourceLine));
        }

        public List<ColumnInfo> Columns => new List<ColumnInfo>(this.configuration.Columns);

        public long LoadedBytes { get; private set; }

        public long TotalBytes { get; private set; }

        protected CsvReaderConfiguration configuration;

        public class CsvReaderConfiguration:CsvFileConfiguration<ColumnInfo>{}
    }
}
