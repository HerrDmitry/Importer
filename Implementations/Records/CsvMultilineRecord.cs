using System;
using System.Collections.Generic;
using System.Text;
using Importer.Interfaces;
using Importer.Records;

namespace Importer.Implementations.Records
{
    public class CsvMultilineRecord:CsvRecord
    {
        public void AddParsers(CsvRecord record)
        {
            if (this.values==null)
            {
                this.values=new Dictionary<string, IParser>();
            }

            foreach (var parser in this.GetValuesInternal(record))
            {
                this.values.Add(parser.Key, parser.Value);
            }
        }
    }
}
