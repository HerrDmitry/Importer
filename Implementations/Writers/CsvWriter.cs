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

        public void Write(IRecord record)
        {
            var builder = new StringBuilder();
            try
            {
                var qualifier = this.configuration.TextQualifierChar;
                var delimiter = this.configuration.DelimiterChar;
                for (var i = 0; i < this.configuration.Columns.Count; i++)
                {
                    var columnInfo = this.configuration.Columns[i];
                    var column = record[columnInfo.Source];
                    if (column.IsFailed)
                    {
                        this.HandleException(record);
                        return;
                    }
                    var s = column.ToString(columnInfo.Format);
                    var tb = new StringBuilder(s);
                    var hasDelimiters = s.IndexOf(delimiter) >= 0;
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
                this.WriteInternal(builder);
                this.recordCounter++;
            }
            catch (Exception ex)
            {
                Logger.GetLogger().ErrorAsync(ex.Message);
            }
        }

        public async Task WriteAsync(IRecord record)
        {
            await Task.Run(() =>
            {
                this.Write(record);
            });
        }

        public override void Close()
        {
            base.Close();
            Logger.GetLogger().InfoAsync($"Processed successfully {this.recordCounter} records, had errors {this.exceptionCounter} records, total {this.recordCounter+this.exceptionCounter} records");
        }

        private void HandleException(IRecord record)
        {
            this.exceptionCounter++;
        }

        private CsvWriterConfiguration configuration;
        private volatile int recordCounter = 0;
        private volatile int exceptionCounter = 0;

        public class CsvWriterConfiguration:Importer.Configuration.CsvFileConfiguration<CsvWriterColumn>
        {
        }

        public class CsvWriterColumn:ColumnInfo{
            [JsonProperty("source")]
            public string Source { get; set; }
        }
    }
}
