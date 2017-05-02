using System;
using System.IO;
using System.Threading.Tasks;
using Importer.Configuration;
using Importer.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Importer.Writers
{
    public class CsvWriter:IWriter
    {
        public CsvWriter(JObject configuration)
        {
            this.configuration = Configuration.Configuration.ParseConfiguration<CsvWriterConfiguration>(configuration);
        }

        public CsvWriter(Stream stream, JObject configuration):this(configuration)
        {
            this.SetDataDestination(stream);
        }

        public async Task FlushAsync()
        {
            await this.writer.FlushAsync();
        }

        public void SetDataDestination(Stream stream)
        {
            this.writer = new StreamWriter(stream);
        }

        public async Task WriteAsync(IRecord record)
        {

            await this.writer.WriteLineAsync();
        }

        public void Close(){
            
        }

        private TextWriter writer;
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
