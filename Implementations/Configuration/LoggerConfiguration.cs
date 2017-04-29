using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Importer.Implementations.Configuration
{
    public class LoggerConfiguration
    {
        [JsonProperty("level")]
        public string Level { get; set; }
    }
}
