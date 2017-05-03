using System;
namespace Importer.Implementations.Parsers
{
    public class BooleanParser : Parser<bool?>
    {
        protected override bool? Parse(out bool isFailed)
        {
            isFailed = false;
            if (this.input.Length == 0)
            {
                return (bool?)null;
            }
            var s = this.input.ToString();
            if (string.IsNullOrWhiteSpace(s))
            {
                return (bool?)null;
            }

            isFailed = !bool.TryParse(this.input.ToString(), out bool parsedValue);
            return parsedValue;
        }

        public override string ToString()
        {
            return this.Value.HasValue?this.Value.Value.ToString():NULL_STRING_VALUE;
        }
    }
}
