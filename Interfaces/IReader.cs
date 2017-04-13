using System;
using System.Collections.Generic;
using System.IO;

namespace Importer.Interfaces
{
    public interface IReader
    {
        IEnumerable<IInputRecord> ReadFromStream(Stream stream);
    }
}