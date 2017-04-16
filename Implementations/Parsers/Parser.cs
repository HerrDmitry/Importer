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
        public virtual string Parse()
        {
            return input;
        }

        public static Parser GetParser(string type, string input)
        {
            switch (type.ToUpper())
            {
                case "STRING":
                    return new StringParser() { input = input };
                case "INTEGER":
                    return new IntegerParser() { input = input };
                case "DATE":
                    return new DateParser() { input = input };
                case "FLOAT":
                    return new FloatParser() { input = input };
            }

            throw new NotSupportedException($"Type {type} is not supported");
        }

        protected string input;
    }

    public abstract class Parser<T>:Parser
    {
        public abstract T Value { get; }
    }
}
