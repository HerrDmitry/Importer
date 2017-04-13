using System;
using System.Collections.Generic;
using System.Text;
using Importer.Interfaces;

namespace Importer.Implementations.Parsers
{
    public abstract class Parser:IParser
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
                    return new StringParser() {input = input};
                case "INTEGER":
                    return new IntegerParser() {input = input};
            }

            throw new NotSupportedException($"Type {type} is not supported");
        }

        protected string input;
    }
}
