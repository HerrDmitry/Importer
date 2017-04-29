using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Implementations.Parsers
{
    public class DateParser : Parser<DateTime>
    {
        protected override DateTime Parse(out bool isFailed){
            isFailed=!DateTime.TryParse(this.input, out DateTime parsedValue);
            return parsedValue;
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
