using System;
namespace Importer.Implementations.Parsers
{
    public class BooleanParser : Parser<bool>
    {
        protected override bool Parse (out bool isFailed){
                    isFailed=!bool.TryParse(this.input, out bool parsedValue);
                return parsedValue;
        }
        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
