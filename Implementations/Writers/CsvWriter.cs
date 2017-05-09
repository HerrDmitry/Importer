using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Importer.Configuration;
using Importer.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Importer.Writers
{
    using Importer.Parsers;

    public class CsvWriter:FileWriter, IWriter
    {
        public CsvWriter(JObject configuration)
        {
            this.configuration = Configuration.Configuration.ParseConfiguration<CsvWriterConfiguration>(configuration);
        }

        public CsvWriter(Stream stream, JObject configuration):this(configuration)
        {
            this.SetDataDestination(stream);
        }

        protected override StringBuilder ConvertRecord(IRecord record)
        {
            var builder = new StringBuilder();
            var qualifier = this.configuration.TextQualifierChar;
            var delimiter = this.configuration.DelimiterChar;
            for (var i = 0; i < this.configuration.Columns.Count; i++)
            {
                var columnInfo = this.configuration.Columns[i];
                var column = record[columnInfo.Source];
                if (column is NotFoundParser)
                {
                    DataDictionary.GetDictionary(this.configuration.References, columnInfo.Source)
                }
                if (column.IsFailed)
                {
                    this.HandleException(record);
                    return null;
                }
                var s = column.ToString(columnInfo.Format);
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

                if (builder.Length > 0)
                {
                    builder.Append(delimiter);
                }
                builder.Append(tb);
            }

            builder.AppendLine();
            return builder;
        }

        private CsvWriterConfiguration configuration;

        public class CsvWriterConfiguration:Importer.Configuration.CsvFileConfiguration<CsvWriterColumn>
        {
        }

        public class CsvWriterColumn:ColumnInfo{
            [JsonProperty("source")]
            public string Source { get; set; }
        }
    }
}
