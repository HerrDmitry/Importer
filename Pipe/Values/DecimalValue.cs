﻿using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Parsers;

namespace Importer.Pipe.Values
{
    public class DecimalValue : Value, IValue<Decimal>, Value.ISetValue<Decimal>
    {
        protected override string ToStringInternal(string format, string nullValue = "")
        {
            return this.IsNull ? nullValue : this.Value.ToString(format);
        }

        public decimal Value { get; private set; }
        IValue<decimal> ISetValue<Decimal>.SetValue(decimal value)
        {
            this.Value = value;
            return this;
        }
    }
}