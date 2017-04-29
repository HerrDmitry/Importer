using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Implementations.Parsers
{
    public class FloatParser : Parser<float>
    {
        protected override float Parse(out bool isFailed){
            isFailed=!float.TryParse(this.input, out float parsedValue);
            return parsedValue;
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
