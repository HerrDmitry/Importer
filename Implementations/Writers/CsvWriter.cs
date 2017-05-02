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

        public async Task WriteAsync(IRecord record)
        {
            var qualifier = this.configuration.TextQualifierChar.ToString();
            var delimiter = this.configuration.DelimiterChar;
            var delimiterS = delimiter.ToString();
            var builder = new StringBuilder();
            for (var i = 0; i < this.configuration.Columns.Count;i++){
                var columnInfo = this.configuration.Columns[i];
                var column = record[columnInfo.Source];
                if(column.IsFailed){
                    this.HandleException(record);
                    return;
                }
                var s = column.ToString(columnInfo.Format);
                var tb = new StringBuilder(s);
                var length = tb.Length;
                tb.Replace(qualifier, qualifier + qualifier);
                if(tb.Length>length || s.Contains(delimiterS)){
                    tb.Insert(0, qualifier).Append(qualifier);
                }

                if(builder.Length>0){
                    builder.Append(delimiter);
                }
                builder.Append(tb);
            }
            await this.writer.WriteLineAsync(builder.ToString());
        }

        private void HandleException(IRecord record){
            
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
