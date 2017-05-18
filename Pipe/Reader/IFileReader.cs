using System;

namespace Importer.Pipe.Reader
{
    using System.Collections.Generic;

    using Importer.Pipe.Record;

    public interface IFileReader:IDisposable
    {
        IEnumerable<DataRecord> ReadData();
    }
}
