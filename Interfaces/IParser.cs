using System;
namespace Importer.Interfaces
{
    public interface IParser
    {
        string Parse();

        string ColumnName { get; }
    }
}
