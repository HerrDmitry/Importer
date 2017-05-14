using System.Collections;
using System.Collections.Generic;
using Importer.Pipe.Parsers;

namespace Importer.Pipe.Writer
{
    public interface IFileWriter
    {
        void Write(IEnumerable<IValue> values);
        void WriteLine(IEnumerable<IValue> values = null);
    }
}