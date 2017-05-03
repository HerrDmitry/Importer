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
                this.Percentage=sr.BaseStream.Position/(double)sr.BaseStream.Length;
                yield return new CsvRecord(this.configuration, sourceLine.ToString());
            }
        }

        public List<ColumnInfo> Columns => new List<ColumnInfo>(this.configuration.Columns);

        public double Percentage { get; private set; } 

        private readonly CsvReaderConfiguration configuration;

        public class CsvReaderConfiguration:CsvFileConfiguration<ColumnInfo>{}
    }
}
