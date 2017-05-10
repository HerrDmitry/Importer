using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Parsers
{
    public abstract class Parser:IParser
    {
        public void SetFormat(string input, string output, string nullStringValue="")
        {
            this.inputFormat = input;
            this.outputFormat = output;
            this.nullStringValue = nullStringValue;
        }

        public abstract bool Parse(string input, out string result);

        protected string inputFormat;

        protected string outputFormat;

        protected string nullStringValue;
    }
}
