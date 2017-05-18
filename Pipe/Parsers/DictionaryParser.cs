using Importer.Pipe.Configuration;

namespace Importer.Pipe.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DictionaryParser
    {
        public DictionaryParser(Column column)
        {
            this.KeyFieldName = column.Name;
            //var dictionaryName=column.Reference.Split(new []{'.'},StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if(!string.IsNullOrWhiteSpace(column.Reference))
            {
                this.dictionary = DataDictionary.Get(column.Reference);
            }
        }

        public Dictionary<string,IValue> GetDictionaryRecord(Dictionary<string, IValue> record)
        {
            if (record.TryGetValue(this.KeyFieldName, out IValue keyValue))
            {
                if (!keyValue.IsFailed && this.dictionary != null && this.dictionary.TryGetValue(keyValue.ToString(), out Dictionary<string, IValue> dict))
                {
                    return dict;
                }
            }

            return new Dictionary<string, IValue>();
        }

        private string KeyFieldName;

        private string DictionaryName;

        private Dictionary<string, Dictionary<string,IValue>> dictionary;
    }
}