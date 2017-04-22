using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Importer.Implementations.Readers;
using Importer.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Importer.Implementations
{
    public class Configuration
    {
        public Configuration(string filePath)
        {
            var configurationJson = File.OpenText(filePath).ReadToEnd();
            var configurationData = JsonConvert.DeserializeObject<ConfigurationData>(configurationJson);
            if (configurationData.Log != null)
            {
                switch (configurationData.Log.Level.ToUpper())
                {
                    case "ERROR":
                        Logger.GetLogger().SetLogginLevel(Logger.LogLevel.Error);
                        break;
                    case "INFO":
                        Logger.GetLogger().SetLogginLevel(Logger.LogLevel.Info);
                        break;
                    case "DEBUG":
                        Logger.GetLogger().SetLogginLevel(Logger.LogLevel.Debug);
                        break;
                }
            }

            this.readers = new Dictionary<string, IReader>();
            configurationData?.Readers?.ForEach(x =>
            {
                var baseConfig = ParseConfiguration<ReaderConfiguration<Column>>(x);
                switch (baseConfig.Type.ToUpper())
                {
                    case "CSV":
                        this.readers[baseConfig.Name] = new CsvReader(x);
                        break;
                }
            });

            var fls = new Dictionary<string, string>();
            foreach (var file in configurationData?.Files){
                fls[file.Key] = file.Value?.ToString();
            }

            this.Files = fls.ToImmutableDictionary();
        }

        private readonly Dictionary<string, IReader> readers;

        public ImmutableDictionary<string, string> Files { get; private set; }

        public ImmutableDictionary<string, IReader> GetReaders()
        {
            return this.readers.ToImmutableDictionary();
        }

        public IReader GetReader(string readerName){
            return this.readers.TryGetValue(readerName, out IReader reader) ? reader : null;
        }

        public Dictionary<string, IWriter> GetWriters()
        {
            return null;
        }

        public class ConfigurationData
        {
            [JsonProperty("readers")]
            public List<JObject> Readers { get; set; }

            [JsonProperty("log")]
            public LoggerConfiguration Log { get; set; }

            [JsonProperty("files")]
            public JObject Files { get; set; }
        }

        public static T ParseConfiguration<T>(JObject config)
        {
            return config.ToObject<T>();
        }

        public class LoggerConfiguration
        {
            [JsonProperty("level")]
            public string Level { get; set; }
        }

        [DebuggerDisplay("{Name} - {Type}")]
        public class ReaderConfiguration<T> where T:Column
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("columns")]
            public List<T> Columns { get; set; }
        }

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
}
