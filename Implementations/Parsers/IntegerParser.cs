using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Implementations.Parsers
{
    public class IntegerParser:Parser<int>
    {
        protected override int Parse(out bool isFailed){
            isFailed=!int.TryParse(this.input, out int parsedValue);
            return parsedValue;
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
