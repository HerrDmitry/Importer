using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Importer.Pipe.Parsers
{
    public class DateParser:Parser,IParser<DateTime>
    {
        public bool Parse(string input, out DateTime result, out bool isNull)
        {
            var isSuccessful = true;
            isNull = false;
            if (string.IsNullOrWhiteSpace(input))
            {
                isNull = true;
                result = DateTime.MinValue;
            }
            else
            {
                isSuccessful = !string.IsNullOrWhiteSpace(this.inputFormat)
                    ? DateTime.TryParseExact(input, this.inputFormat, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out result)
                    : DateTime.TryParse(input, out result);
            }

            return isSuccessful;
        }

        public override bool Parse(string input, out string result)
        {
            var isSuccessful = this.Parse(input, out DateTime r, out bool isNull);
            result = isSuccessful ? (isNull ? null : r.ToString(this.outputFormat)) : this.nullStringValue;

            return isSuccessful;
        }
    }
}
