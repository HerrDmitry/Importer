using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    public class DateParser:Parser,IParser<DateTime>
    {
        public bool Parse(string input, out IValue<DateTime> result)
        {
            var isSuccessful = true;
            if (string.IsNullOrWhiteSpace(input))
            {
                result = Value.GetValue(DateTime.MinValue,true);
            }
            else
            {
                isSuccessful = !string.IsNullOrWhiteSpace(this.inputFormat)
                    ? DateTime.TryParseExact(input, this.inputFormat, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out DateTime r)
                    : DateTime.TryParse(input, out r);
                result = Value.GetValue(r, false);
            }

            return isSuccessful;
        }

        public override bool Parse(string input, out IValue result)
        {
            var isSuccessful = this.Parse(input, out IValue<DateTime> r);
            result = r;
            return isSuccessful;
        }
    }
}
