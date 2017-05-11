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

        public static Parser GetParser(string sourceName, ColumnInfo column, string input)
        {
            Parser parser = null;
            switch (column.Type)
            {
                case "string":
                    parser = Factory<StringParser>.GetInstance();
                    break;
                case "integer":
                    parser = Factory<IntegerParser>.GetInstance();
                    break;
                case "date":
                    parser = Factory<DateParser>.GetInstance();
                    break;
                case "float":
                    parser = Factory<FloatParser>.GetInstance();
                    break;
                case "boolean":
                    parser = Factory<BooleanParser>.GetInstance();
                    break;
                case "text":
                    parser = Factory<TextParser>.GetInstance();
                    break;
                default:
                    throw new NotSupportedException($"Type {column.Type} is not supported");
            }

            parser.input = input;
            parser.column = column;
            return parser;
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public virtual string ToString(string format)
        {
            return this.ToString();
        }

        public void Release()
        {
            if (this is StringParser)
            {
                Factory<StringParser>.ReleaseInstance(this);
            }
            if (this is IntegerParser)
            {
                Factory<IntegerParser>.ReleaseInstance(this);
            }
            if (this is DateParser)
            {
                Factory<DateParser>.ReleaseInstance(this);
            }
            if (this is FloatParser)
            {
                Factory<FloatParser>.ReleaseInstance(this);
            }
            if (this is BooleanParser)
            {
                Factory<BooleanParser>.ReleaseInstance(this);
            }
            if (this is TextParser)
            {
                Factory<TextParser>.ReleaseInstance(this);
            }
        }

        public void Clear()
        {
            this.input = null;
            this.isParsed = false;
        }

        public string ColumnName => this.column.Name;

        protected string input;
        protected ColumnInfo column;
        protected bool isParsed;

        private static class Factory<T> where T : Parser, new()
        {
            private static readonly ConcurrentBag<T> availableInstances = new ConcurrentBag<T>();

            public static T GetInstance()
            {
                if (!availableInstances.TryTake(out T parser))
                {
                    parser = new T();
                }

                return parser;
            }

            public static void ReleaseInstance(Parser instance)
            {
                instance.Clear();
                availableInstances.Add((T)instance);
            }
        }
    }

    public abstract class Parser<T>:Parser
    {
        private object locker = new object();
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
