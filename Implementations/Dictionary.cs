using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Importer.Implementations.Parsers;
using Importer.Interfaces;

namespace Importer.Implementations
{
    public class Dictionary
    {
        public Dictionary(string keyColumnName, IEnumerable<IInputRecord> records)
        {
            var dictionary=new Dictionary<string,IInputRecord>();

            foreach (var inputRecord in records)
            {
                inputRecord.GetValues()
            }

            this.DictionaryItems=new ImmutableDictionary<string, IInputRecord>(null);
        }

        public IImmutableDictionary<string, IInputRecord> DictionaryItems;

        public IInputRecord this[string key] => DictionaryItems[key];

        public IInputRecord TryGetValueDefault(string key, IInputRecord defaultValue = null)
        {
            return this.DictionaryItems.TryGetValue(key, out IInputRecord value) ? value : defaultValue;
        }
    }
}
