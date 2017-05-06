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

        List<Importer.Configuration.ColumnInfo> Columns { get; }

        long LoadedBytes { get; }
        long TotalBytes { get; }
    }
}