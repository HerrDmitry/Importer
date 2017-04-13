﻿using System;
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
            var testData = JsonConvert.DeserializeObject<TestData>(configurationJson);
            this.readers = new Dictionary<string, IReader>();
            configurationData?.Readers?.ForEach(x =>
            {
                var baseConfig = ParseConfiguration<ReaderConfiguration>(x);
                switch (baseConfig.Type.ToUpper())
                {
                    case "CSV":
                        this.readers[baseConfig.Name] = new CsvReader(x);
                        break;
                }
            });
        }

        private readonly Dictionary<string, IReader> readers;

        public ImmutableDictionary<string, IReader> GetReaders()
        {
            return this.readers.ToImmutableDictionary();
        }

        public Dictionary<string, IWriter> GetWriters()
        {
            return null;
        }

        public class TestData
        {
            [JsonProperty("readers")]
            public List<JObject> Readers { get; set; }
        }

        public class ConfigurationData
        {
            [JsonProperty("readers")]
            public List<JObject> Readers { get; set; }
        }

        public static T ParseConfiguration<T>(JObject config) where T : ReaderConfiguration
        {
            return config.ToObject<T>();
        }

        [DebuggerDisplay("{Name} - {Type}")]
        public class ReaderConfiguration
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }

        [DebuggerDisplay("{Name} - {Type}")]
        public class Column
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }
    }
}