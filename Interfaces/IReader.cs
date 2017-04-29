using System;
using System.Collections.Generic;
using System.IO;
using Importer.Implementations;

namespace Importer.Interfaces
{
    public interface IReader
    {
        void SetDataSource(Stream stream);
        IEnumerable<IRecord> ReadData();

        List<Configuration.Column> Columns { get; }
    }
}