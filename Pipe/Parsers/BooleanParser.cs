using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    using Pipe.Configuration;

    public class BooleanParser:Parser,IParser<bool>
    {
        public BooleanParser(Column column):base(column)
        {
        }

        IValue<bool> IParser<bool>.Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new BooleanValue(false, true, false, this.Column);
            }

            var isSuccessful = bool.TryParse(input, out bool r);
            return new BooleanValue(r, false, !isSuccessful, this.Column);
        }

        public override IValue Parse(string input)
        {
            return  ((IParser<bool>)this).Parse(input);
        }
    }
}
