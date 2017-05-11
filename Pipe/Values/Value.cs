using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Parsers;

namespace Importer.Pipe.Values
{
    public abstract class Value:IValue
    {
        public interface ISetValue<T>
        {
            IValue<T> SetValue(T value);
        }

        public static IValue<T> GetValue<T>(T value, bool isNull)
        {
            switch (typeof(T).Name)
            {
                case "System.Boolean":
                    return ((ISetValue<T>) new BooleanValue{IsNull = isNull}).SetValue(value);
                default:
                    throw new ArgumentOutOfRangeException($"Value of type {typeof(T).Name} is not supported.");
            }
        }

        public void SetFormat(string format)
        {
            this.format = format;
        }

        public void SetNullValue(string nullValue)
        {
            this.nullValue = nullValue;
        }

        protected abstract string ToStringInternal(string format, string nullValue = "");

        public string ToString(string format, string nullValue = "")
        {
            if (this.lastString == null || format != this.lastUsedFormat || this.lastUsedNullString != nullValue)
            {
                this.lastString = this.ToStringInternal(format, nullValue);
                this.lastUsedFormat = format;
                this.lastUsedNullString = nullValue;
            }

            return this.lastString;
        }

        public bool IsNull { get; protected set; }

        private string lastString=null;
        private string lastUsedFormat = null;
        private string lastUsedNullString = null;
        public override string ToString()
        {
            return this.ToString(this.format, this.nullValue);
        }

        protected string format = null;
        protected string nullValue = "";
    }
}
