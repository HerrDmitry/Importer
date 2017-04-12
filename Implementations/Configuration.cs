using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Importer.Implementations.Readers;
using Importer.Interfaces;
using Newtonsoft.Json;

namespace Importer.Implementations
{
    public class Configuration
    {
        public Configuration(string filePath)
        {
            var configurationJson = File.OpenText(filePath).ReadToEnd();
            var configurationData = JsonConvert.DeserializeObject<ConfigurationData>(configurationJson);
            var testData = JsonConvert.DeserializeObject<TestData>(configurationJson);
            this.readers=new Dictionary<string, IReader>();
            configurationData?.Readers?.ForEach(x =>
            {
                switch (x.Type.ToUpper())
                {
                    case "CSV":
                        this.readers[x.Name] = new CsvReader("");
                        break;
                }
            });
        }

        private Dictionary<string, IReader> readers;
        public Dictionary<string, IReader> GetReaders()
        {
            return null;
        }

        public Dictionary<string, IWriter> GetWriters()
        {
            return null;
        }

        public class TestData
        {
            [JsonProperty("readers")]
            public List<> Readers { get; set; }
        }

        public class ConfigurationData
        {
            [JsonProperty("readers")]
            public List<Reader> Readers { get; set; }

            public class Reader
            {
                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("type")]
                public string Type { get; set; }

                
            }
        }
    }
}
