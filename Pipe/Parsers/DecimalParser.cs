using Importer.Pipe.Values;

namespace Importer.Pipe.Parsers
{
    using Configuration;

    public class DecimalParser:Parser,IParser<decimal>
    {
        public DecimalParser(Column column)
            : base(column)
        {
        }

        IValue<decimal> IParser<decimal>.Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new DecimalValue(0, true, false, this.Column);
            }

            var isSuccessful = decimal.TryParse(input, out decimal r);
            return new DecimalValue(r, false, !isSuccessful, this.Column);
        }

        public override IValue Parse(string input)
        {
            return ((IParser<decimal>)this).Parse(input);
        }
    }
}
