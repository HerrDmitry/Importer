using System.Collections.Generic;
using Importer.Configuration;
using Importer.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Importer.Readers
{
    public class CsvMultilineReader:CsvReader
    {
        public CsvMultilineReader(JObject configuration)
        {
            this.configuration = Configuration.Configuration.ParseConfiguration<CsvMultilineReaderConfiguration>(configuration);
        }

/*        public override IEnumerable<IRecord> ReadData()
        {
            foreach (var line in this.ReadLines())
            {

            }
        }*/

        public class CsvMultilineReaderConfiguration:CsvReaderConfiguration
        {
            [JsonProperty("rows")]
            public List<Row> Rows { get; set; }

            public class Row
            {
                [JsonProperty("columns")]
                public List<ColumnInfo> Columns { get; set; }
            }
        }

    }
}