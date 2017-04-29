using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Importer.Interfaces;

namespace Importer.Implementations.Parsers
{
    [DebuggerDisplay("{" + nameof(input) + "}")]
    public abstract class Parser : IParser
    {
        public string ColumnName { get; private set; }

        public abstract bool IsFailed { get; }

        public static Parser GetParser(string name, string type, string input)
        {
            switch (type.ToUpper())
            {
                case "STRING":
                    return new StringParser() { ColumnName = name, input = input };
                case "INTEGER":
                    return new IntegerParser() { ColumnName = name, input = input };
                case "DATE":
                    return new DateParser() { ColumnName = name, input = input };
                case "FLOAT":
                    return new FloatParser() { ColumnName = name, input = input };
                case "BOOLEAN":
                    return new BooleanParser() { ColumnName = name, input = input };
                default:
                    throw new NotSupportedException($"Type {type} is not supported");
            }
        }

        public new abstract string ToString();

        public virtual string ToString(string format)
        {
            return this.ToString();
        }

        protected string input;
    }

    public abstract class Parser<T>:Parser
    {
        private object locker = new object();
        private bool isParsed;
        private T parsedValue;
        private bool isFailed;
        public T Value { 
            get{
                if(!this.isParsed){
                    this.ParseInternal();
                }

                if(this.IsFailed){
                    throw new ArgumentException($"Failed to parse {this.input} into {typeof(T).Name}");
                }

                return this.parsedValue;
            } 
        }

        public override bool IsFailed{
            get{
                if(!isParsed){
                    this.ParseInternal();
                }

                return this.isFailed;
            }
        }

        private void ParseInternal(){
            lock (this.locker)
            {
                if (this.isParsed) { 
                    return; 
                }
                this.parsedValue = this.Parse(out bool isFailed);
                this.isFailed = isFailed;
                this.isParsed = true;
            }
        }

        protected abstract T Parse(out bool isFailed);
    }
}
