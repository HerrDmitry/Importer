using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Importer.Configuration
{
    [DebuggerDisplay("{Name} - {Type}")]
    public abstract class FileConfiguration<T> where T : ColumnInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("columns")]
        public List<T> Columns { get; set; }
    }
}
