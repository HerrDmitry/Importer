using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Implementations.Parsers
{
    public class FloatParser : Parser<float>
    {
        public override float Value => float.Parse(this.Parse());
        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
