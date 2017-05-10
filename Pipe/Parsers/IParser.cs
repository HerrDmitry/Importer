using System;
using System.Collections.Generic;
using System.Text;

namespace Importer.Pipe.Parsers
{
    public interface IParser
    {
        void SetFormat(string input, string output, string nullStringValue=null);
        bool Parse(string input, out string result);
    }

    public interface IParser<T>:IParser
    {
        bool Parse(string input, out T result, out bool isNull);
    }
}
