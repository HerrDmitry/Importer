using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    using Configuration;

    public class StringParser:Parser,IParser<string>
    {
        public StringParser(Column column)
            : base(column)
        {
        }

        IValue<string> IParser<string>.Parse(string input)
        {
            return new StringValue(input, input == null, false, this.Column);
        }

        public override IValue Parse(string input)
        {
            return ((IParser<string>)this).Parse(input);
        }
    }
}
