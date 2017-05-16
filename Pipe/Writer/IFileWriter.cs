using System.Collections;
using System.Collections.Generic;
using Importer.Pipe.Parsers;

namespace Importer.Pipe.Writer
{
    public interface IFileWriter
    {
        void Write(Dictionary<string, IValue> values);

        void WriteLine(Dictionary<string, IValue> values = null);
    }
}