using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Implementations.Parsers
{
    public class FloatParser : Parser<float?>
    {
        protected override float? Parse(out bool isFailed){
            isFailed = false;
            if (this.input.Length == 0)
            {
                return (float?)null;
            }
            var s = this.input.ToString();
            if (string.IsNullOrWhiteSpace(s))
            {
                return (float?)null;
            }
            isFailed = !float.TryParse(s, out float parsedValue);
            return parsedValue;
        }

        public override string ToString()
        {
            return this.Value.HasValue?this.Value.Value.ToString():NULL_STRING_VALUE;
        }
    }
}
