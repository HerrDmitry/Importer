using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Parsers
{
    using Configuration;

    public interface IValue
    {
        string ToString(string format, string nullValue="");
        bool IsNull { get; }
        bool IsFailed { get; }

        Column Column { get; }
    }

    public interface IValue<out T> : IValue
    {
        T Value { get; }
    }
}
