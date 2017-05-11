using System;
using System.Collections.Generic;
using System.Text;
using Importer.Pipe.Parsers;

namespace Importer.Pipe.Values
{
    public abstract class Value:IValue
    {
        public static IValue<T> GetValue<T>(T value)
        {
            switch (typeof(T).Name)
            {
                case "System.Boolean":
                    return new BooleanValue().SetValue(value);
            }
        }

        protected abstract IValue<T> SetValue<T>(T value);

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
