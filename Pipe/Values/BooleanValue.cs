﻿using Importer.Pipe.Parsers;

namespace Importer.Pipe.Values
{
    using Configuration;

    public struct BooleanValue : IValue<bool>
    {
        public BooleanValue(bool value, bool isNull, bool isFailed, Column column)
        {
            this.Column = column;
            this.Value = value;
            this.IsFailed = isFailed;
            this.IsNull = isNull;
        }

        public bool Value { get; }

        public string ToString(string format, string nullValue = "")
        {
            return this.IsNull ? nullValue : this.Value.ToString();
        }

        public override string ToString()
        {
            return this.ToString(null);
        }

        public bool IsNull { get; }
        public bool IsFailed { get; }
        public Column Column { get; }
    }
}
