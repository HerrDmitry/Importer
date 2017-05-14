using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    using Importer.Pipe.Configuration;

    public class BooleanParser:Parser,IParser<bool>
    {
        public BooleanParser(Column column):base(column)
        {
        }

        IValue<bool> IParser<bool>.Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Value.GetValue(false, true, false, this.Column);
            }

            var isSuccessful = bool.TryParse(input, out bool r);
            return Value.GetValue(r, false, !isSuccessful, this.Column);
        }

        public override IValue Parse(string input)
        {
            return  ((IParser<bool>)this).Parse(input);
        }
    }
}
