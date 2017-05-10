using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Importer.Configuration;
using Importer.Implementations.Parsers;
using Importer.Interfaces;
using static Importer.Readers.CsvReader;

namespace Importer.Records
{
    public class CsvRecord : Record<CsvRecord>
    {
        protected Dictionary<string,IParser> values=null;

        protected override Dictionary<string,IParser> GetValuesInternal()
        {
            if (this.values?.Count > 0)
            {
                return this.values;
            }

            this.values = this.config.GetColumnsWithFullNames().ToDictionary(x => x.FullName, x => (IParser)Parser.GetParser(this.config.Name, x.Column, this.GetNext()));

            if (this.references == null)
            {
                this.references=new List<IRecord>();
            }

            this.references.Clear();
            this.config.References?.ForEach(r =>
            {
                var key = string.Concat(this.config.Name, ".", r.Field);
                if (this.values.TryGetValue(key, out IParser keyValue))
                {
                    if (!keyValue.IsFailed && DataDictionary.GetDictionary(r.Reference).TryGetValue(keyValue.ToString(), out IRecord dictionaryRecord))
                    {
                        this.references.Add(dictionaryRecord);
                    }
                    else
                    {
                        key = key;
                    }
                }
                else
                {
                    key = key;
                }
            });

            return this.values;
        }

        public override void InitializeRecord<TCI>(FileConfiguration<TCI> config, StringBuilder source, long rowNumber)
        {
            this.source = source.ToString();
            this.index = 0;
            this.length = this.source.Length;
            this.config = config as CsvReaderConfiguration;
            this.values?.Clear();
            this.references?.Clear();
            this.RowNumber = rowNumber;
        }

        public override void ClearRecord()
        {
            this.source = null;
            this.index = 0;
            this.length = 0;
            this.config = null;
            if (this.values != null)
            {
                foreach (var parser in this.values)
                {
                    parser.Value.Release();
                }
            }

            this.values?.Clear();
            this.references?.Clear();
        }

        public override string Source => this.source;

        private string GetNext()
        {
            if (this.index >= this.length)
            {
                return "";
            }

            var expected = this.config.DelimiterChar;
            if (this.source[this.index] == this.config.DelimiterChar)
            {
                this.index++;
                return "";
            }
            int start,end;
            if (this.source[this.index] == this.config.TextQualifierChar)
            {
                expected = this.config.TextQualifierChar;
                start = this.index + 1;
            }
            else
            {
                start = this.index;
            }

            end = start;
            var done = false;
            while (!done)
            {
                this.index++;
                var idx = this.source.IndexOf(expected, this.index);
                if (idx < 0)
                {
                    idx = this.source.Length;
                }
                end = idx - start;
                this.index = idx;

                if (this.index < this.length)
                {
                    if (this.index<this.length-1 && this.source[this.index]==this.config.TextQualifierChar){
                        if (this.source[this.index + 1] == this.config.TextQualifierChar)
                        {
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
            return this.source.Substring(start,end).Replace(this.config.TextQualifier,this.config.TextQualifierDouble);
        }

        private string source;
        private int index;
        private int length;
        private CsvReaderConfiguration config;
    }
}
