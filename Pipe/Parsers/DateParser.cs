using System;
using System.Globalization;
using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    using Pipe.Configuration;

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
                return new DateValue(DateTime.MinValue, true, false, this.Column);
            }

            var isSuccessful = !string.IsNullOrWhiteSpace(this.inputFormat)
                                   ? DateTime.TryParseExact(input, this.inputFormat, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out DateTime r)
                                   : DateTime.TryParse(input, out r);
            return new DateValue(r, false, !isSuccessful, this.Column);
        }

        public override IValue Parse(string input)
        {
            return ((IParser<DateTime>)this).Parse(input);
        }
    }
}
