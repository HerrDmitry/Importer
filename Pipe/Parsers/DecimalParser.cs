using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Parsers
{
    public class DecimalParser:Parser,IParser<decimal>
    {
        public bool Parse(string input, out decimal result, out bool isNull)
        {
            var isSuccessful = true;
            isNull = false;
            if (string.IsNullOrWhiteSpace(input))
            {
                isNull = true;
                result = 0;
            }
            else
            {
                isSuccessful = decimal.TryParse(input, out result);
            }

            return isSuccessful;
        }

        public override bool Parse(string input, out string result)
        {
            var isSuccessful = this.Parse(input, out decimal r, out bool isNull);
            result = isSuccessful ? (isNull ? null : r.ToString(this.outputFormat)) : this.nullStringValue;

            return isSuccessful;
        }
    }
}
