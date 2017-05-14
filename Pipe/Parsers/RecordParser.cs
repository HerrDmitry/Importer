using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Parsers
{
    using System.Linq;

    using Importer.Pipe.Configuration;

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

                }
            }
            this.parsers = parsers.ToArray();
        }

        public List<IValue> Parse(IEnumerable<string> source)
        {
            var idx = 0;
            return source.Take(this.parsers.Length).Select(col => this.parsers[idx++].Parse(col)).ToList();
        }

        public Dictionary<string, IValue> ParseToDictionary(IEnumerable<string> source)
        {
            return this.Parse(source).ToDictionary(x => x.Column.Name, x => x);
        }

        private IParser[] parsers;
    }
}
