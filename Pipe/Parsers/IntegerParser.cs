using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    using Importer.Pipe.Configuration;

    public class IntegerParser:Parser,IParser<int>
    {
        public IntegerParser(Column column):base(column)
        {
        }

        IValue<int> IParser<int>.Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Value.GetValue(0, true, false, this.Column);
            }

            var isSuccessful = int.TryParse(input, out int r);
            return Value.GetValue(r, false, !isSuccessful, this.Column);
        }

        public override IValue Parse(string input)
        {
            return ((IParser<int>)this).Parse(input);
        }
    }
}
