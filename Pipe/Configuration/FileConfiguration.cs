using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Configuration
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class FileConfiguration
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        public static FileConfiguration Read(JObject rawConfig)
        {
            var baseConfig = rawConfig.ToObject<FileConfiguration>();
            switch (baseConfig.Type)
            {
                case "CSV": return rawConfig.ToObject<CsvFileConfiguration>().NormalizeColumnNames();
            }

            throw new ArgumentOutOfRangeException($"File of type {baseConfig.Type} is not supported.");
        }

        public virtual List<KeyValuePair<string,string>> GetReferences()
        {
            return new List<KeyValuePair<string,string>>();
        }

        protected virtual FileConfiguration NormalizeColumnNames()
        {
            return this;
        }
    }
}
