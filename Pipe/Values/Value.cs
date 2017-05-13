using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Parsers;

namespace Importer.Pipe.Values
{
    using Importer.Pipe.Configuration;

    public abstract class Value:IValue
    {
        public interface ISetValue<T>
        {
            IValue<T> SetValue(T value);
        }

        public static IValue<T> GetValue<T>(T value, bool isNull, bool isFailed, Column column)
        {
            if (typeof(T) == typeof(bool))
            {
                return ((ISetValue<T>)new BooleanValue(column) { IsNull = isNull, IsFailed = isFailed }).SetValue(value);
            }
            if(typeof(T) == typeof(int))
            {
                return ((ISetValue<T>)new IntegerValue(column) { IsNull = isNull, IsFailed = isFailed }).SetValue(value);
            }
            if(typeof(T) == typeof(decimal))
            {
                return ((ISetValue<T>)new DecimalValue(column) { IsNull = isNull, IsFailed = isFailed }).SetValue(value);
            }
            if(typeof(T) == typeof(DateTime))
            {
                return ((ISetValue<T>)new DateValue(column) { IsNull = isNull, IsFailed = isFailed }).SetValue(value);
            }
            if(typeof(T) == typeof(string))
            {
                return ((ISetValue<T>)new StringValue(column) { IsNull = isNull, IsFailed = isFailed }).SetValue(value);
            }

            throw new ArgumentOutOfRangeException($"Value of type {typeof(T).Name} is not supported.");
        }

        public Value(Column column)
        {
            this.column = column;
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

        public bool IsFailed { get; protected set; }

        public Column Column => this.column;

        private string lastString=null;
        private string lastUsedFormat = null;
        private string lastUsedNullString = null;
        public override string ToString()
        {
            return this.ToString(this.format, this.nullValue);
        }

        protected string format = null;
        protected string nullValue = "";

        protected Column column;
    }
}
