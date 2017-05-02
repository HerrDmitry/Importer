using System;
using System.Collections.Generic;
using System.IO;
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

        public CsvReader(JObject configuration)
        {
            this.configuration = Configuration.Configuration.ParseConfiguration<CsvReaderConfiguration>(configuration);
        }

        public void SetDataSource(Stream source){
            this.dataSource = source;
        }

        public IEnumerable<IRecord> ReadData()
        {
            var sr = new StreamReader(this.dataSource);
            var qualifier = this.configuration.TextQualifierChar;
            while (!sr.EndOfStream)
            {
                var sourceLine = new StringBuilder();
                lock (this.dataSource)
                {
                    var qualifierCount = 0;
                    while (qualifierCount == 0 || qualifierCount % 2 != 0)
                    {
                        var line = sr.ReadLine();
                        sourceLine.Append(line);

                        for (var i = 0; i < line.Length; i++)
                        {
                            if (line[i] == qualifier)
                            {
                                qualifierCount++;
                            }
                        }

                        if (qualifierCount == 0)
                        {
                            break;
                        }
                    }

                }

                yield return new CsvRecord(this.configuration, sourceLine.ToString());
            }
        }

        public List<Configuration.ColumnInfo> Columns => new List<Configuration.ColumnInfo>(this.configuration.Columns);

        private readonly CsvReaderConfiguration configuration;

        public class CsvReaderConfiguration:Importer.Configuration.CsvFileConfiguration<ColumnInfo>{}
    }
}
