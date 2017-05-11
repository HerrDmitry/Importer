using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Parsers;

namespace Importer.Pipe.Values
{
    public class IntegerValue : Value, IValue<int>, Value.ISetValue<int>
    {
        protected override string ToStringInternal(string format, string nullValue = "")
        {
            return this.IsNull ? nullValue : this.Value.ToString(format);
        }

        public int Value { get; private set; }
        IValue<int> ISetValue<int>.SetValue(int value)
        {
            this.Value = value;
            return this;
        }
    }
}
