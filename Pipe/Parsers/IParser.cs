using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Parsers
{
    public interface IParser
    {
        void SetInputFormat(string input);
    }

    public interface IParser<T>:IParser
    {
        bool Parse(string input, IValue<T> result);
    }
}
