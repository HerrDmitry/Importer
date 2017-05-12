using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe
{
    using Importer.Pipe.Parsers;

    public class DataDictionary
    {
        private DataDictionary()
        {
        }

        private Dictionary<string, IValue> dictionary;

        public IValue this[string key] => this.dictionary.TryGetValue(key, out IValue value) ? value : null;

        public static void LoadDictionary(string reference, List<IParser> columns, IEnumerable<IEnumerable<string>> data)
        {
            var dictionary=new DataDictionary();

        }

        public static DataDictionary GetDictionary(string reference)
        {
            return dataDictionaries.TryGetValue(reference, out DataDictionary dict) ? dict : null;
        }

        private static Dictionary<string, DataDictionary> dataDictionaries = new Dictionary<string, DataDictionary>();
    }
}
