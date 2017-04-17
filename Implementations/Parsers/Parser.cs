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

        public string ColumnName { get; private set; }

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
