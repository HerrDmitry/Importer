using System;
using Importer.Configuration;

namespace Importer.Interfaces
{
    public interface IParser
    {
        bool IsFailed { get; }

        string ToString(string format);
    }
}
