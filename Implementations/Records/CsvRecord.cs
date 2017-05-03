using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Importer.Implementations.Parsers;
using Importer.Interfaces;
using static Importer.Readers.CsvReader;

namespace Importer.Records
{
    public class CsvRecord : Record
    {
        public CsvRecord(CsvReaderConfiguration config, StringBuilder source)
        {
            this.source = source;
            this.index = 0;
            this.length = this.source.Length;
            this.config = config;
        }

        private ImmutableDictionary<string,IParser> values=null;

        protected override ImmutableDictionary<string,IParser> GetValuesInternal()
        {
            return this.values ?? (this.values =
                                   this.config.GetColumnsWithFullNames().ToImmutableDictionary(x => x.FullName,
                                                                             x => (IParser)Parser.GetParser(this.config.Name, x.Column, this.GetNext())));
        }

        private string GetNext()
        {
            if (this.index >= this.length)
            {
                return null;
            }

            var next = new StringBuilder();
            var expected = this.config.DelimiterChar;
            if (this.source[this.index] == this.config.DelimiterChar)
            {
                this.index++;
                return "";
            }
            if (this.source[this.index] == this.config.TextQualifierChar)
            {
                expected = this.config.TextQualifierChar;
            }
            else
            {
                next.Append(this.source[this.index]);
            }

            var done = false;
            while (!done)
            {
                this.index++;

                while (this.index < this.length && this.source[this.index] != expected)
                {
                    next.Append(this.source[this.index++]);
                }
                if (this.index < this.length)
                {
                    if (this.index<this.length-1 && this.source[this.index]==this.config.TextQualifierChar){
                        if (this.source[this.index + 1] == this.config.TextQualifierChar)
                        {
                            next.Append(this.config.TextQualifierChar);
                            this.index++;
                            continue;
                        } else if(this.source[this.index+1]==this.config.DelimiterChar){
                            this.index++;
                            this.index++;
                            done=true;
                        } else{
                            throw new FormatException("Row in incorrect format");
                        }
                    }
                    else
                    {
                        done = true;
                        this.index++;
                    }
                } else{
                    done = true;
                }
            }
            return next.ToString();
        }

        private StringBuilder source;
        private int index;
        private int length;
        private CsvReaderConfiguration config;
    }
}
