using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Importer.Pipe.Reader
{
    public interface IFileReader
    {
        IEnumerable<IEnumerable<string>> ReadData();
    }
}
