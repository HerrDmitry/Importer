using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Implementations.Parsers
{
    public class DateParser : Parser<DateTime>
    {
        public override DateTime Value => DateTime.Parse(this.input);
    }
}
