using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Importer.Interfaces
{
    public interface IOutputRecord
    {
        void Write(Stream stream);
    }
}
