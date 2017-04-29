using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Implementations.Parsers
{
    public class IntegerParser:Parser<int>
    {
        public override int Value => int.Parse(this.Parse());
        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
