using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Importer.Configuration;
using Importer.Interfaces;

namespace Importer.Implementations.Parsers
{
    [DebuggerDisplay("{" + nameof(input) + "}")]
    public abstract class Parser : IParser
    {
        public ColumnInfo Column { get; private set; }

        public string SourceName { get; private set; }

        public abstract bool IsFailed { get; }

        public static Parser GetParser(string sourceName, ColumnInfo column, string input)
        {
            switch (column.Type.ToUpper())
            {
                case "STRING":
                    return new StringParser() { SourceName=sourceName, Column = column, input = input };
                case "INTEGER":
                    return new IntegerParser() { SourceName=sourceName, Column = column, input = input };
                case "DATE":
                    return new DateParser() { SourceName=sourceName, Column = column, input = input };
                case "FLOAT":
                    return new FloatParser() { SourceName=sourceName, Column = column, input = input };
                case "BOOLEAN":
                    return new BooleanParser() { SourceName=sourceName, Column = column, input = input };
                default:
                    throw new NotSupportedException($"Type {column.Type} is not supported");
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
