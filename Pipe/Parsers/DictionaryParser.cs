using Importer.Pipe.Configuration;

namespace Importer.Pipe.Parsers
{
    public class DictionaryParser
    {
        public DictionaryParser(Column column) : base(column)
        {
        }

        public override IValue Parse(string input)
        {
            throw new System.NotImplementedException();
        }

        IValue<string> IParser<string>.Parse(string input)
        {
            throw new System.NotImplementedException();
        }
    }
}