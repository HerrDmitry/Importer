using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Importer.Implementations.Configuration
{
    [DebuggerDisplay("{Name} - {Type}")]
    public class Column
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }
    }
}
