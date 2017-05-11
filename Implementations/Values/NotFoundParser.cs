using System;
using System.Collections.Generic;
using System.Text;
using Importer.Configuration;
using Importer.Implementations.Parsers;

namespace Importer.Parsers
{
    using Importer.Interfaces;

    public class NotFoundParser: Parser{

        public NotFoundParser(ColumnInfo column)
        {
            this.column = column;
        }

        public override bool IsFailed => true;

        public override string ToString(string format)
        {
            return null;
        }
    }
}
