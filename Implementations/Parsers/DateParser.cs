using System;

namespace Importer.Implementations.Parsers
{
    public class DateParser : Parser<DateTime?>
    {
        protected override DateTime? Parse(out bool isFailed){
            isFailed = false;
            if (this.input.Length == 0)
            {
                return (DateTime?)null;
            }
            var s = this.input.ToString();
            if (string.IsNullOrWhiteSpace(s))
            {
                return (DateTime?)null;
            }

            isFailed = !DateTime.TryParse(s, out DateTime parsedValue);
            return parsedValue;
        }

        public override string ToString()
        {
            return this.Value.HasValue?this.Value.ToString():NULL_STRING_VALUE;
        }

        public override string ToString(string format)
        {
            return this.Value.HasValue?this.Value.Value.ToString(format):NULL_STRING_VALUE;
        }
    }
}
