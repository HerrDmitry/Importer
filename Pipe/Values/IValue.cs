using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Parsers
{
    public interface IValue
    {
        void SetFormat(string format);
        void SetNullValue(string nullValue);
        string ToString(string format, string nullValue="");
        bool IsNull { get; }
    }

    public interface IValue<T> : IValue
    {
        T Value { get; }
    }
}
