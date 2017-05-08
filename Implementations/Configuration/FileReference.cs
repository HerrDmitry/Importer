using Newtonsoft.Json;

namespace Importer.Configuration
{
    public class FileReference
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }
    }
}
