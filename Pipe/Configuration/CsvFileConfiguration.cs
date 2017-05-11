using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Configuration
{
    using Newtonsoft.Json;

    public class CsvFileConfiguration : FileConfiguration
    {
        [JsonProperty("columns")]
        public List<Column> Columns { get; set; }

        [JsonProperty("delimiter")]
        public string Delimiter { get; set; }

        [JsonProperty("textQualifier")]
        public string TextQualifier { get; set; }
    }
}
 