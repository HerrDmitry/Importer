using System.Collections.Generic;

namespace Importer.Pipe.Configuration
{
    using Newtonsoft.Json;

    public class CsvMultilineFileConfiguration :CsvFileConfiguration
    {
        [JsonProperty("rows")]
        public List<CsvRow> Rows { get; set; }

        public class CsvRow
        {
            [JsonProperty("columns")]
            public List<Column> Columns { get; set; }
        }
    }
}
