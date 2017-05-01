using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Importer.Implementations.Configuration
{
    [DebuggerDisplay("{Name} - {Type}")]
    public class ReaderConfiguration<T> where T : ColumnInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("columns")]
        public List<T> Columns { get; set; }
    }
}
