using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    using Importer.Pipe.Configuration;

    public class DecimalParser:Parser,IParser<decimal>
    {
        public DecimalParser(Column column)
            : base(column)
        {
        }

        IValue<decimal> IParser<decimal>.Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Value.GetValue((decimal)0, true, false, this.column);
            }

            var isSuccessful = decimal.TryParse(input, out decimal r);
            return Value.GetValue(r, false, !isSuccessful, this.column);
        }

        public override IValue Parse(string input)
        {
            return ((IParser<decimal>)this).Parse(input);
        }
    }
}
