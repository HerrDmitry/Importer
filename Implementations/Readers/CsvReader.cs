﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Importer.Implementations.Records;
using Importer.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Importer.Implementations.Readers
{
    public class CsvReader : IReader
    {
        public CsvReader(JObject configuration)
        {
            this.configuration = Configuration.ParseConfiguration<CsvReaderConfiguration>(configuration);
            if (string.IsNullOrEmpty(this.configuration.Delimiter))
            {
                this.configuration.Delimiter = ",";
            }
        }

        public IEnumerable<IInputRecord> ReadFromStream(Stream stream)
        {
            var sr=new StreamReader(stream);
            while (!sr.EndOfStream)
            {
                var sourceLine=new StringBuilder();
                var qualifierCount = 0;
                var qualifier = this.configuration.TextQualifierChar;
                while (qualifierCount == 0 || qualifierCount % 2 == 0)
                {
                    var line = sr.ReadLine();
                    sourceLine.Append(line);
                    foreach (var c in line)
                    {
                        if (c == qualifier)
                        {
                            qualifierCount++;
                        }
                    }

                    if (qualifierCount == 0)
                    {
                        break;
                    }
                }


                yield return new CsvInputRecord(this.configuration.Columns, sourceLine.ToString(), this.configuration.Delimiter[0],
                    this.configuration.TextQualifierChar);
            }
        }

        private readonly CsvReaderConfiguration configuration;

        public class CsvReaderConfiguration:Configuration.ReaderConfiguration
        {
            [JsonProperty("delimiter")]
            public string Delimiter { get; set; }

            [JsonProperty("textQualifier")]
            public string TextQualifier { get; set; }

            [JsonIgnore]
            public char TextQualifierChar => !string.IsNullOrEmpty(this.TextQualifier) ? this.TextQualifier[0] : default(char);

            [JsonProperty("footer")]
            public int? Footer { get; set; }

            [JsonProperty("header")]
            public int? Header { get; set; }

            [JsonProperty("columns")]
            public List<CsvInputRecord.CsvColumn> Columns { get; set; }
        }
    }
}