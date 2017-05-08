using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Interfaces
{
    public interface ITextReader
    {
        IEnumerable<string> GetLines();
        long LoadedBytes { get; }
        long TotalBytes { get; }
    }
}
