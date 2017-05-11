using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Parsers;

namespace Importer.Pipe.Values
{
    public class BooleanValue : Value, IValue<bool>, Value.ISetValue<bool>
    {
        IValue<bool> ISetValue<bool>.SetValue(bool value)
        {
            this.Value = value;
            return this;
        }

        protected override string ToStringInternal(string format, string nullValue = "")
        {
            return this.IsNull ? nullValue : this.Value.ToString();
        }

        public bool Value { get; private set; }
    }
}
