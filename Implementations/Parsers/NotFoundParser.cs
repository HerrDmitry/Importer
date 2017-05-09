using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Parsers
{
    using Importer.Interfaces;

    public class NotFoundParser: IParser{
        public bool IsFailed => true;

        public string ToString(string format)
        {
            return null;
        }

        public void Release()
        {
        }
    }
}
