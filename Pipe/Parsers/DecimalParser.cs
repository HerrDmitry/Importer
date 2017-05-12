﻿using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    using Importer.Pipe.Configuration;

    public class DecimalParser:Parser,IParser<decimal>
    {
        public DecimalParser(Column column)
            : base(column)
        {
        }

        public bool Parse(string input, out IValue<decimal> result)
        {
            var isSuccessful = true;
            if (string.IsNullOrWhiteSpace(input))
            {
                result = Value.GetValue((decimal)0, true, this.column);
            }
            else
            {
                isSuccessful = decimal.TryParse(input, out decimal r);
                result = Value.GetValue(r, false, this.column);
            }

            return isSuccessful;
        }

        public override bool Parse(string input, out IValue result)
        {
            var isSuccessful = this.Parse(input, out IValue<decimal> r);
            result = r;

            return isSuccessful;
        }
    }
}
