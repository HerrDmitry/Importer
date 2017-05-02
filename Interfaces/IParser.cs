using System;
using Importer.Configuration;

namespace Importer.Interfaces
{
    public interface IParser
    {
        string SourceName { get; }
        ColumnInfo Column { get; }

        bool IsFailed { get; }
    }
}
