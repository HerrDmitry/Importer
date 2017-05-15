using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Configuration
{
    using System.Diagnostics;

    using Newtonsoft.Json;

    [DebuggerDisplay("{Name} - {Type}")]
    public class Column
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }
}
