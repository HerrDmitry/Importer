using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Importer.Pipe.Reader
{
    using Importer.Pipe.Parsers;

    public interface IFileReader:IDisposable
    {
        IEnumerable<IEnumerable<IValue>> ReadData();
    }
}
