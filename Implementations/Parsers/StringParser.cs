using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Implementations.Parsers
{
    public class StringParser : Parser<string>
    {
        protected override string Parse(out bool isFailed){
            isFailed = false;
            return this.input.ToString();
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}