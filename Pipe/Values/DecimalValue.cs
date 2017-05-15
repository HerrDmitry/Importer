using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Parsers;

namespace Importer.Pipe.Values
{
    using Importer.Pipe.Configuration;

    public class DecimalValue : IValue<Decimal>
    {
        public DecimalValue(decimal value, bool isNull, bool isFailed, Column column)
        {
            this.Column = column;
            this.Value = value;
            this.IsFailed = isFailed;
            this.IsNull = isNull;
        }

        public decimal Value { get; }

        public string ToString(string format, string nullValue = "")
        {
            return this.IsNull ? nullValue : this.Value.ToString(format);
        }

        public override string ToString()
        {
            return this.ToString(null);
        }

        public bool IsNull { get; }
        public bool IsFailed { get; }
        public Column Column { get; }    }
}
