﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Importer.Readers;
using Importer.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Importer.Writers;

namespace Importer.Configuration
{
    public class Configuration
    {
        public Configuration(string filePath)
        {
            if(!File.Exists(filePath)){
                var message=$"Configuration file \"{filePath}\" not found";
                throw new FileNotFoundException(message);
            }
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
                var baseConfig = ParseConfiguration<FileConfiguration<ColumnInfo>>(x);
                switch (baseConfig.Type.ToUpper())
                {
                    case "CSV":
                        this.readers[baseConfig.Name] = new CsvReader(x);
                        break;
                }
            });

            this.writers = new Dictionary<string, IWriter>();
            configurationData?.Writers?.ForEach(x => {
                var baseConfig = ParseConfiguration<FileConfiguration<ColumnInfo>>(x);
                switch(baseConfig.Type.ToUpper()){
                    case "CSV":
                        this.writers[baseConfig.Name] = new CsvWriter(x);
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

        private readonly Dictionary<string, IWriter> writers;
        public ImmutableDictionary<string, IWriter> GetWriters()
        {
            return this.writers.ToImmutableDictionary();
        }

        public class ConfigurationData
        {
            [JsonProperty("readers")]
            public List<JObject> Readers { get; set; }

            [JsonProperty("log")]
            public LoggerConfiguration Log { get; set; }

            [JsonProperty("files")]
            public JObject Files { get; set; }

            [JsonProperty("writers")]
            public List<JObject> Writers { get; set; }
        }

        public static T ParseConfiguration<T>(JObject config)
        {
            return config.ToObject<T>();
        }
    }
}