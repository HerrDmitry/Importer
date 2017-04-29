using System;
namespace Importer.Implementations.Parsers
{
    public class BooleanParser : Parser<bool>
    {
        public override bool Value => bool.Parse(this.Parse());
        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
