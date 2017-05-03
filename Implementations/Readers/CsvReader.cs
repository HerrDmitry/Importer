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
            var counter = 0;
            while (!sr.EndOfStream /*&& counter<20000*/)
            {
                lock (this.dataSource)
                {
                    var sourceLine = new StringBuilder();
                    var qualifierCount = 0;
                    while (qualifierCount == 0 || qualifierCount % 2 != 0)
                    {
                        var line = sr.ReadLine();
                        sourceLine.Append(line).AppendLine();

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
                    this.Percentage = sr.BaseStream.Position / (double)sr.BaseStream.Length;
                    counter++;
                    yield return Record<CsvRecord>.Factory.GetRecord(this.configuration, sourceLine);
                }
            }
        }

        public List<ColumnInfo> Columns => new List<ColumnInfo>(this.configuration.Columns);

        public double Percentage { get; private set; } 

        private readonly CsvReaderConfiguration configuration;

        public class CsvReaderConfiguration:CsvFileConfiguration<ColumnInfo>{}
    }
}
