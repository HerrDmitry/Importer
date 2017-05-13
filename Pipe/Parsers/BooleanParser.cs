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

        public IValue<bool> Parse(string input)
        {
            var isSuccessful = true;
            if (string.IsNullOrWhiteSpace(input))
            {
                result = Value.GetValue(false, true, this.column);

            }
            else
            {
                isSuccessful = bool.TryParse(input, out bool r);
                result = Value.GetValue(r, false, this.column);
            }

            return isSuccessful;
        }

        public override bool Parse(string input, out IValue result)
        {
            var isSuccessful = this.Parse(input, out IValue<bool> r);
            result = r;

            return isSuccessful;
        }
    }
}
