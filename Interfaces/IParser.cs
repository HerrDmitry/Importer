using System;
namespace Importer.Interfaces
{
    public interface IParser
    {
        string ColumnName { get; }

        bool IsFailed { get; }
    }
}
