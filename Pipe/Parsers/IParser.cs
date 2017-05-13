using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Parsers
{
    public interface IParser
    {
        void SetInputFormat(string input);
        IValue Parse(string input);
    }

    public interface IParser<T>:IParser
    {
        IValue<T> Parse(string input);
    }
}
