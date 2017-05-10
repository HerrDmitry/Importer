using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Parsers
{
    public class StringParser:Parser,IParser<string>
    {
        public bool Parse(string input, out string result, out bool isNull)
        {
            result = input;
            isNull = input == null;
            return true;
        }

        public override bool Parse(string input, out string result)
        {
            result = input ?? this.nullStringValue;
            return true;
        }
    }
}
