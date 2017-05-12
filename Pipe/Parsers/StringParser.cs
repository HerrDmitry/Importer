using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    using Importer.Pipe.Configuration;

    public class StringParser:Parser,IParser<string>
    {
        public StringParser(Column column)
            : base(column)
        {
        }

        public bool Parse(string input, out IValue<string> result)
        {
            result = Value.GetValue(input, input == null, this.column);
            return true;
        }

        public override bool Parse(string input, out IValue result)
        {
            this.Parse(input, out IValue<string> r);
            result = r;
            return true;
        }
    }
}
