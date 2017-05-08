using System;
using System.Collections.Generic;
using System.IO;
using Importer.Implementations;

namespace Importer.Interfaces
{
    public interface IReader
    {
        void SetDataSource(ITextReader reader);
        IEnumerable<IRecord> ReadData();

        List<Importer.Configuration.ColumnInfo> Columns { get; }
        List<Configuration.FileReference> References { get; }

        long LoadedBytes { get; }
        long TotalBytes { get; }
    }
}