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
            this.parsers = this.parsers.ToArray();
        }

        public List<IValue> Parse(IEnumerable<string> source)
        {
            
        }

        private IParser[] parsers;
    }
}
