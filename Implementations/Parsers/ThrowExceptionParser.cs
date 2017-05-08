using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Implementations.Parsers
{
    using Importer.Interfaces;

    public class ThrowExceptionParser: IParser{
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
