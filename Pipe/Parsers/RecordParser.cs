using System.Collections.Generic;

namespace Importer.Pipe.Parsers
{
    using System.Linq;

    using Configuration;

    public class RecordParser
    {
        public RecordParser(IEnumerable<Column> columns)
        {
            var parsers = new List<IParser>();
            foreach (var column in columns)
            {
                parsers.Add(Parser.GetParser(column));
                if (!string.IsNullOrWhiteSpace(column.Reference))
                {
                    this.dictionaries.Add(new DictionaryParser(column));
                }
            }
            this.parsers = parsers.ToArray();
        }

        private List<IValue> ParseInternal(IEnumerable<string> source)
        {
            var idx = 0;
            return source.Take(this.parsers.Length).Select(col => this.parsers[idx++].Parse(col)).ToList();
        }

        public Dictionary<string, IValue> Parse(IEnumerable<string> source)
        {
            var values = this.ParseInternal(source).ToDictionary(x => x.Column.Name, x => x);
            for (var i = 0; i < this.dictionaries.Count; i++)
            {
                var dictValues = this.dictionaries[i].GetDictionaryRecord(values);
                if (dictValues != null)
                {
                    foreach (var dictValue in dictValues)
                    {
                        values.Add(dictValue.Key, dictValue.Value);
                    }
                }
            }

            return values;
        }

        private IParser[] parsers;

        private List<DictionaryParser> dictionaries = new List<DictionaryParser>();
    }
}
