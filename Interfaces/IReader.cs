using System;
using System.Collections.Generic;
using System.IO;
using Importer.Implementations;

namespace Importer.Interfaces
{
    public interface IReader
    {
        IEnumerable<IInputRecord> ReadFromStream(Stream stream);

        List<Configuration.Column> Columns { get; }
    }
}