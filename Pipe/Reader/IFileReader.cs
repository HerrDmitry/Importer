using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Importer.Pipe.Reader
{
    public interface IFileReader:IDisposable
    {
        IEnumerable<IEnumerable<string>> ReadData();
    }
}
