using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Importer.Pipe.Configuration
{
    using System.Linq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ImporterConfiguration
    {
        private List<JObject> readersRaw;
        private JObject filesRaw;

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

        private List<JObject> writersRaw;

        [JsonProperty("writers")]
        public List<JObject> WritersRaw
        {
            get => this.writersRaw;
            set
            {
                this.writersRaw = value;
                this.Writers = value.Select(x => FileConfiguration.Read(x)).ToArray();
            }
        }

        [JsonIgnore]
        public IEnumerable<FileConfiguration> Writers { get; private set; }

        [JsonProperty("files")]
        public JObject FilesRaw
        {
            get => this.filesRaw;
            set
            {
                this.filesRaw = value;
                var fls = new Dictionary<string, string>();
                foreach (var file in this.filesRaw)
                {
                    fls[file.Key] = file.Value?.ToString();
                }

                this.Files = fls;

            }
        }

        [JsonIgnore]
        public Dictionary<string, string> Files { get; private set; }

        public static ImporterConfiguration ReadConfiguration(string filePath)
        {
            var configurationJson = File.OpenText(filePath).ReadToEnd();
            var configurationData = JsonConvert.DeserializeObject<ImporterConfiguration>(configurationJson);
            return configurationData;
        }
    }
}