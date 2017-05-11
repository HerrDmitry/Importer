using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    public class BooleanParser:Parser,IParser<bool>
    {
        public bool Parse(string input, out IValue<bool> result)
        {
            var isSuccessful = true;
            if (string.IsNullOrWhiteSpace(input))
            {
                result = Value.GetValue(false, true);

            }
            else
            {
                isSuccessful = bool.TryParse(input, out bool r);
                result = Value.GetValue(r, false);
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
