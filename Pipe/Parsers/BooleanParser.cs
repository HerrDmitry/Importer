using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Parsers
{
    public class BooleanParser:Parser,IParser<bool>
    {
        public bool Parse(string input, out bool result, out bool isNull)
        {
            var isSuccessful = true;
            isNull = false;
            if (string.IsNullOrWhiteSpace(input))
            {
                isNull = true;
                result = false;
            }
            else
            {
                isSuccessful = bool.TryParse(input, out result);
            }

            return isSuccessful;
        }

        public override bool Parse(string input, out string result)
        {
            var isSuccessful = this.Parse(input, out bool r, out bool isNull);
            result = isSuccessful ? (isNull ? null : r.ToString()) : this.nullStringValue;

            return isSuccessful;
        }
    }
}
