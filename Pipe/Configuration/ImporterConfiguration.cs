using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Configuration
{
    using System.Linq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ImporterConfiguration
    {
        private List<JObject> readersRaw;

        [JsonProperty("readers")]
        public List<JObject> ReadersRaw
        {
            get => this.readersRaw;
            set
            {
                this.readersRaw = value;
                this.Readers = value.Select(x => FileConfiguration.Read(x)).ToArray();
            }
        }

        [JsonIgnore]
        public IEnumerable<FileConfiguration> Readers { get; private set; }
    }
}
