﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml;
using Importer.Configuration;
using Importer.Implementations.Parsers;
using Importer.Interfaces;
using static Importer.Readers.CsvReader;

namespace Importer.Records
{
    public class CsvRecord : Record<CsvRecord>
    {
        private ImmutableDictionary<string,IParser> values=null;

        protected override ImmutableDictionary<string,IParser> GetValuesInternal()
        {
            return this.values ?? (this.values =
                                   this.config.GetColumnsWithFullNames().ToImmutableDictionary(x => x.FullName,
                                                                             x => (IParser)Parser.GetParser(this.config.Name, x.Column, this.GetNext())));
        }

        public override void InitializeRecord<TCI>(FileConfiguration<TCI> config, StringBuilder source)
        {
            this.source = source.ToString();
            this.sourceChar = new char[source.Length];
            source.CopyTo(0,this.sourceChar,0,source.Length);
            this.index = 0;
            this.length = this.source.Length;
            this.config = config as CsvReaderConfiguration;
            this.values = null;
        }

        public override void ClearRecord()
        {
            this.source = null;
            this.sourceChar = null;
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
        }

        private StringBuilder GetNext()
        {
            if (this.index >= this.length)
            {
                throw new ArgumentOutOfRangeException("index is out of bounds");
            }

            var next = new StringBuilder();
            var expected = this.config.DelimiterChar;
            if (this.source[this.index] == this.config.DelimiterChar)
            {
                this.index++;
                return new StringBuilder();
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
                next.Append(this.sourceChar,start, idx - start);
                this.index = idx;

/*
                while (this.index < this.length && this.source[this.index] != expected)
                {
                    this.index++;
                }

                next.Append(this.source, start, this.index - start);
*/
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
        private char[] sourceChar;
        private int index;
        private int length;
        private CsvReaderConfiguration config;
    }
}
