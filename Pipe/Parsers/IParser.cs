using Importer.Pipe.Configuration;

namespace Importer.Pipe.Parsers
{
    public interface IParser
    {
        void SetInputFormat(string input);
        IValue Parse(string input);
        Column Column { get; }
    }

    public interface IParser<T>:IParser
    {
        IValue<T> Parse(string input);
    }
}
