using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Parsers;

namespace Importer.Pipe.Values
{
    using Importer.Pipe.Configuration;

    public class DateValue : Value, IValue<DateTime>, Value.ISetValue<DateTime>
    {
        public DateValue(Column column)
            : base(column)
        {
        }

        protected override string ToStringInternal(string format, string nullValue = "")
        {
            return this.IsNull ? nullValue : this.Value.ToString(format);
        }

        public DateTime Value { get; private set; }

        IValue<DateTime> ISetValue<DateTime>.SetValue(DateTime value)
        {
            this.Value = value;
            return this;
        }
    }
}
