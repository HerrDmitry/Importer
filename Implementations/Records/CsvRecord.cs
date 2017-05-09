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
            if (this.values == null)
            {
                this.values = this.config.GetColumnsWithFullNames().ToDictionary(x => x.FullName, x => (IParser)Parser.GetParser(this.config.Name, x.Column, this.GetNext()));
            }

            this.references = config.References?.SelectMany(r =>
            {
                var key = string.Concat(this.config.Name, ".", r.Field);
                if (this.values.TryGetValue(key, out IParser keyValue))
                {
                    if (!keyValue.IsFailed && DataDictionary.GetDictionary(r.Reference).TryGetValue(keyValue.ToString(), out IRecord dictionaryRecord)){
                        return dictionaryRecord.GetValues();
                    }
                }
                return new Dictionary<string, IParser>();
            }).ToDictionary(x => x.Key, x => x.Value);

            return this.values;
        }

        protected Dictionary<string,IParser> GetValuesInternal(CsvRecord record)
        {
            return record.values;
        }

        public override void InitializeRecord<TCI>(FileConfiguration<TCI> config, StringBuilder source)
        {
            this.source = source.ToString();
            this.index = 0;
            this.length = this.source.Length;
            this.config = config as CsvReaderConfiguration;
            this.values = null;
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

            this.values = null;
            this.references = null;
        }

        private StringBuilder GetNext()
        {
            var next = new StringBuilder();
            if (this.index >= this.length)
            {
                return next;
            }

            var expected = this.config.DelimiterChar;
            if (this.source[this.index] == this.config.DelimiterChar)
            {
                this.index++;
                return next;
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
                var start = this.index;
                var idx = this.source.IndexOf(expected, this.index);
                if (idx < 0)
                {
                    idx = this.source.Length;
                }
                next.Append(this.source.Substring(start, idx - start));
                this.index = idx;

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
            return next;
        }

        private string source;
        private int index;
        private int length;
        private CsvReaderConfiguration config;
    }
}
