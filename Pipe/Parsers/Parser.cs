using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Parsers
{
    using Importer.Pipe.Configuration;

    public abstract class Parser:IParser
    {
        public void SetInputFormat(string input)
        {
            this.inputFormat = input;
        }

        public void SetOutputFormat(string output)
        {
            this.outputFormat = output;
        }

        public abstract bool Parse(string input, out IValue result);

        public static IParser GetParser(Column column)
        {
            IParser parser;
            switch (column.Type)
            {
                case "string":
                    parser = new StringParser();
                    break;
                case "boolean":
                case "bool":
                    parser = new BooleanParser();
                    break;
                case "date":
                case "datetime":
                case "time":
                    parser = new DateParser();
                    break;
                case "decimal":
                case "float":
                case "double":
                case "money":
                    parser = new DecimalParser();
                    break;
                case "integer":
                case "int":
                    parser = new IntegerParser();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Column type {column.Type} for column {column.Name} is not supported.");
            }

            parser.SetInputFormat(column.Format);
            return parser;
        }

        protected string inputFormat;

        protected string outputFormat;

        protected string nullStringValue;
    }
}
