using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Interfaces
{
    public interface IRecord
    {
        IEnumerable<IParser> GetValues();

        IParser this[string columnName] { get; }
    }
}
