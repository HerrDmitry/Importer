using System;
using System.Collections.Generic;
using System.Text;
using Importer.Interfaces;

namespace Importer.Implementations
{
    public class Configuration
    {
        public Configuration(string filePath)
        {
        }

        public Dictionary<string, IReader> GetReaders()
        {
        }

        public Dictionary<string, IWriter> GetWriters()
        {
        }
    }
}
