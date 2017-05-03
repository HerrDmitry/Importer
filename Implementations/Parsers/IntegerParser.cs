using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Implementations.Parsers
{
    public class IntegerParser:Parser<int?>
    {
        protected override int? Parse(out bool isFailed){
            isFailed = false;
            if (this.input.Length == 0)
            {
                return (int?)null;
            }
            var s = this.input.ToString();
            if (string.IsNullOrWhiteSpace(s))
            {
                return (int?)null;
            }

            isFailed = !int.TryParse(s, out int parsedValue);
            return parsedValue;
        }

        public override string ToString()
        {
            return this.Value.HasValue?this.Value.Value.ToString():NULL_STRING_VALUE;
        }
    }
}
