﻿using System.Collections.Generic;
using System.Linq;
using Importer.Configuration;
using Importer.Implementations.Parsers;
using Importer.Implementations.Records;
using Importer.Interfaces;
using Importer.Records;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Importer.Readers
{
    using System.Collections.Concurrent;

    public class CsvMultilineReader : CsvReader
    {
        public CsvMultilineReader(JObject configuration)
        {
            this.configuration =
                Configuration.Configuration.ParseConfiguration<CsvMultilineReaderConfiguration>(configuration);
        }

        public override IEnumerable<IRecord> ReadData()
        {
            var config = (CsvMultilineReaderConfiguration) this.configuration;
            using (var enumerator = this.ReadLines().GetEnumerator())
            {
                long rowNumber = 0;
                while (true)
                {
                    CsvMultilineRecord record = null;
                    foreach (var row in config.Rows)
                    {
                        if (!enumerator.MoveNext())
                        {
                            yield break;
                        }

                        var partialRecord = Record<CsvRecord>.Factory.GetRecord(config.ToConfiguration(row), enumerator.Current, 0);
                        var parsers = partialRecord.GetValues();
                        if (record == null)
                        {
                            record=new CsvMultilineRecord();
                        }

                        record.AddParsers(partialRecord);
                        if (parsers.Where(x=>x.Value is TextParser).Any(x => x.Value.IsFailed))
                        {
                            break;
                        }
                    }

                    if (record != null)
                    {
                        record.RowNumber = rowNumber++;
                        yield return record;
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
        }

        public class CsvMultilineReaderConfiguration : CsvReaderConfiguration
        {
            [JsonProperty("rows")]
            public List<Row> Rows { get; set; }

            public class Row
            {
                [JsonProperty("columns")]
                public List<ColumnInfo> Columns { get; set; }
            }

            public CsvReaderConfiguration ToConfiguration(Row row)
            {
                return new CsvReaderConfiguration
                {
                    Delimiter = this.Delimiter,
                    TextQualifier = this.TextQualifier,
                    Columns = row.Columns,
                    Name = this.Name
                };
            }
        }

    }
}