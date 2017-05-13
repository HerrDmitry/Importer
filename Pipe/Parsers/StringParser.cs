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

        IValue<string> IParser<string>.Parse(string input)
        {
            return Value.GetValue(input, input == null, false, this.column);
        }

        public override IValue Parse(string input)
        {
            return ((IParser<string>)this).Parse(input);
        }
    }
}
