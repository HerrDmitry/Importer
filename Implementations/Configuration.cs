using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Importer.Interfaces;
using Newtonsoft.Json;

namespace Importer.Implementations
{
    public class Configuration
    {
        public Configuration(string filePath)
        {
            var configurationJson = File.OpenText(filePath).ReadToEnd();
            var converter = JsonConvert.DeserializeObject(configurationJson);
        }

        public Dictionary<string, IReader> GetReaders()
        {
            return null;
        }

        public Dictionary<string, IWriter> GetWriters()
        {
            return null;
        }
    }
}
