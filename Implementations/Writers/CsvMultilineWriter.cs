using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Importer.Interfaces;
using Importer.Writers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Importer.Implementations.Writers
{
    public class CsvMultilineWriter:CsvWriter
    {
        public CsvMultilineWriter(JObject configuration) : base(configuration)
        {
            this.configuration = Configuration.Configuration.ParseConfiguration<CsvMultilineWriterConfiguration>(configuration);
        }

        public CsvMultilineWriter(Stream stream, JObject configuration) : this(configuration)
        {
            this.SetDataDestination(stream);
        }

        public class CsvMultilineWriterConfiguration : Importer.Configuration.CsvFileConfiguration<CsvWriterColumn>
        {
            [JsonProperty("rows")]
            public List<Row> Rows { get; set; }

            public class Row
            {
                [JsonProperty("columns")]
                public List<CsvWriterColumn> Columns { get; set; }
            }
        }

        protected override StringBuilder ConvertRecord(IRecord record)
        {
            if (this.configuration.Rows == null || this.configuration.Rows.Count==0)
            {
                return base.ConvertRecord(record);
            }

            var builder = new StringBuilder();
            var qualifier = this.configuration.TextQualifierChar;
            var delimiter = this.configuration.DelimiterChar;
            for (var r = 0; r < this.configuration.Rows.Count; r++)
            {
                var newRow = true;
                var row = this.configuration.Rows[r];
                for (var i = 0; i < row.Columns.Count; i++)
                {
                    var columnInfo = row.Columns[i];
                    string s;
                    if (!string.IsNullOrWhiteSpace(columnInfo.Source))
                    {
                        var column = record[columnInfo.Source];
                        if (column.IsFailed)
                        {
                            this.HandleException(record);
                            return null;
                        }
                        s = column.ToString(columnInfo.Format);
                    }
                    else if (!string.IsNullOrWhiteSpace(columnInfo.Text))
                    {
                        s = columnInfo.Text;
                    }
                    else
                    {
                        this.HandleException(record);
                        return null;
                    }
                    var tb = new StringBuilder(s);
                    var hasDelimiters = s.IndexOf(delimiter) >= 0 || s.IndexOf("\r", StringComparison.Ordinal) >= 0 || s.IndexOf("\n", StringComparison.Ordinal) > 0;
                    var hasQualifier = s.IndexOf(qualifier) >= 0;
                    if (hasQualifier || hasDelimiters)
                    {
                        if (hasQualifier)
                        {
                            var qs = qualifier.ToString();
                            tb.Replace(qs, qs + qs);
                        }
                        tb.Insert(0, qualifier).Append(qualifier);
                    }

                    if (!newRow)
                    {
                        builder.Append(delimiter);
                    }
                    builder.Append(tb);
                    newRow = false;
                }

                builder.AppendLine();
            }
            return builder;
        }

        private CsvMultilineWriterConfiguration configuration;


    }
}
