using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Importer.Implementations.Parsers;
using Importer.Interfaces;

namespace Importer.Implementations.Records
{
    public class CsvInputRecord : IInputRecord
    {
        public CsvInputRecord(List<CsvColumn> columns, string source, char delimiter=',', char textQualifier=default(char))
        {
            this.source = source;
            this.textQualifier = textQualifier;
            this.index = 0;
            this.delimiter = delimiter;
            this.length = this.source.Length;
            this.columns = columns;
        }

        public IEnumerable<IParser> GetValues()
        {
            return this.GetValuesInternal().Values.ToList();
        }

        public IParser this[string columnName] => this.GetValuesInternal()[columnName];

        private ImmutableDictionary<string,IParser> values=null;

        private ImmutableDictionary<string,IParser> GetValuesInternal()
        {
            return this.values ?? (this.values =
                       columns.ToImmutableDictionary(x => x.Name,
                           x => (IParser) Parser.GetParser(x.Name, x.Type, this.GetNext())));
        }

        private string GetNext()
        {
            if (this.index >= this.length)
            {
                return null;
            }

            var next = new StringBuilder();
            var expected = this.delimiter;
            if (this.source[this.index] == this.delimiter)
            {
                this.index++;
                return "";
            }
            if (this.source[this.index] == this.textQualifier)
            {
                expected = this.textQualifier;
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
                    if (this.index<this.length-1 && this.source[this.index]==this.textQualifier){
                        if (this.source[this.index + 1] == this.textQualifier)
                        {
                            next.Append(this.textQualifier);
                            this.index++;
                            continue;
                        } else if(this.source[this.index+1]==this.delimiter){
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

        private string source;
        private int index;
        private char textQualifier;
        private char delimiter;
        private int length;
        private List<CsvColumn> columns;

        public class CsvColumn : Configuration.Column
        {
        }
    }
}
