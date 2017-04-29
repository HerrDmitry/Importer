using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Implementations.Parsers
{
    public class StringParser : Parser<string>
    {
        public override string Value => this.Parse();
        public override string ToString()
        {
            return this.Value;
        }
    }
}