using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Parsers
{
    public interface IParser
    {
        void SetInputFormat(string input);
        void SetOutputFormat(string output);
        bool Parse(string input, out string result);
    }

    public interface IParser<T>:IParser
    {
        bool Parse(string input, out T result, out bool isNull);
    }
}
