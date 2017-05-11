using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    public class IntegerParser:Parser,IParser<int>
    {
        public bool Parse(string input, out IValue<int> result)
        {
            var isSuccessful = true;
            if (string.IsNullOrWhiteSpace(input))
            {
                result = Value.GetValue(0, true);
            }
            else
            {
                isSuccessful = int.TryParse(input, out int r);
                result = Value.GetValue(r, false);
            }
            return isSuccessful;
        }

        public override bool Parse(string input, out IValue result)
        {
            var isSuccessful = this.Parse(input, out IValue<int> r);

            result = r;
            return isSuccessful;
        }
    }
}
