using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    using Configuration;

    public class IntegerParser:Parser,IParser<int>
    {
        public IntegerParser(Column column):base(column)
        {
        }

        IValue<int> IParser<int>.Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new IntegerValue(0, true, false, this.Column);
            }

            var isSuccessful = int.TryParse(input, out int r);
            return new IntegerValue(r, false, !isSuccessful, this.Column);
        }

        public override IValue Parse(string input)
        {
            return ((IParser<int>)this).Parse(input);
        }
    }
}
