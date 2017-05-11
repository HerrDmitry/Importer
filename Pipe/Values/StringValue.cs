using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Parsers;

namespace Importer.Pipe.Values
{
    public class StringValue : Value, IValue<string>, Value.ISetValue<string>
    {
        protected override string ToStringInternal(string format, string nullValue = "")
        {
            return this.IsNull ? nullValue : this.Value;
        }

        public string Value { get; private set; }
        IValue<string> ISetValue<string>.SetValue(string value)
        {
            this.Value = value;
            return this;
        }
    }
}
