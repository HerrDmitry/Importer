using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Parsers;

namespace Importer.Pipe.Values
{
    public class BooleanValue:Value,IValue<bool>
    {
        public BooleanValue()
        {
        }

        protected override IValue<bool> SetValue(bool value)
        {
            this.Value = value;
        }

        protected override string ToStringInternal(string format, string nullValue = "")
        {
            throw new NotImplementedException();
        }

        public bool Value { get; }
    }
}
