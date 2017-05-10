using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Importer.Pipe.Parsers
{
    public class IntegerParser:Parser,IParser<int>
    {
        public bool Parse(string input, out int result, out bool isNull)
        {
            var isSuccessful = true;
            isNull = false;
            if (string.IsNullOrWhiteSpace(input))
            {
                result = 0;
                isNull = true;
            }
            else
            {
                isSuccessful = int.TryParse(input, out result);
            }
            return isSuccessful;
        }

        public override bool Parse(string input, out string result)
        {
            var isSuccessful = this.Parse(input, out int r, out bool isNull);

            result = isSuccessful ? (isNull ? null : r.ToString(this.outputFormat)) : this.nullStringValue;
            return isSuccessful;
        }
    }
}
