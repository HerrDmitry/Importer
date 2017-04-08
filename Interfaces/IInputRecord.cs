using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Interfaces
{
    public interface IInputRecord
    {
        IEnumerable<string> GetValues();
    }
}
