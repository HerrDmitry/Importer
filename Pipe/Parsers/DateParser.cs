using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    using Importer.Pipe.Configuration;

    public class DateParser:Parser,IParser<DateTime>
    {
        public DateParser(Column column)
            : base(column)
        {
        }

        IValue<DateTime> IParser<DateTime>.Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Value.GetValue(DateTime.MinValue, true, false, this.column);
            }

            var isSuccessful = !string.IsNullOrWhiteSpace(this.inputFormat)
                                   ? DateTime.TryParseExact(input, this.inputFormat, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out DateTime r)
                                   : DateTime.TryParse(input, out r);
            return Value.GetValue(r, false, !isSuccessful, this.column);
        }

        public override IValue Parse(string input)
        {
            return ((IParser<DateTime>)this).Parse(input);
        }
    }
}
