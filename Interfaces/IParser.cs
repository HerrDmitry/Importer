using System;
using Importer.Implementations.Configuration;

namespace Importer.Interfaces
{
    public interface IParser
    {
        ColumnInfo Column { get; }

        bool IsFailed { get; }
    }
}
