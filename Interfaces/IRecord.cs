using System;
using System.Collections.Generic;
using System.Text;
using Importer.Configuration;

namespace Importer.Interfaces
{
    public interface IRecord
    {
        IEnumerable<IParser> GetValues();

        IParser this[string columnName] { get; }

        void Release();
    }
}
