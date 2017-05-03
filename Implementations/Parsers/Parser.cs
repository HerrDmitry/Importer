using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Importer.Configuration;
using Importer.Interfaces;

namespace Importer.Implementations.Parsers
{
    [DebuggerDisplay("{" + nameof(input) + "}")]
    public abstract class Parser : IParser
    {
        public abstract bool IsFailed { get; }

        public static Parser GetParser(string sourceName, ColumnInfo column, StringBuilder input)
        {
            Parser parser = null;
            switch (column.Type)
            {
                case "string":
                    parser = Factory<StringParser>.GetInstance();
                    break;
                case "integer":
                    parser = Factory<IntegerParser>().GetInstance();
                    break;
                case "date":
                    return new DateParser() { input = input };
                case "float":
                    return new FloatParser() { input = input };
                case "boolean":
                    return new BooleanParser() { input = input };
                default:
                    throw new NotSupportedException($"Type {column.Type} is not supported");
            }

            parser.input = input;
            return parser;
        }

        public new abstract string ToString();

        public virtual string ToString(string format)
        {
            return this.ToString();
        }

        public abstract void Release();

        protected StringBuilder input;

        private class Factory<T> where T : Parser, new()
        {
            private const int MAX_INSTANCE_COUNTER = 1000;
            private static int instanceCounter = 0;
            private static readonly ConcurrentBag<T> availableInstances = new ConcurrentBag<T>();

            public static T GetInstance()
            {
                T parser = null;
                while (parser == null)
                {
                    if (!availableInstances.TryTake(out parser))
                    {
                        if (instanceCounter < MAX_INSTANCE_COUNTER)
                        {
                            parser = new T();
                        }
                        else
                        {
                            Thread.Sleep(50);
                        }
                    }
                }

                return parser;
            }
        }
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
        protected const string NULL_STRING_VALUE = "N/A";
    }
}
