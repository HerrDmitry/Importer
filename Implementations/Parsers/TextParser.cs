namespace Importer.Implementations.Parsers
{
    public class TextParser : Parser<string>
    {
        public override string ToString()
        {
            return this.Value;
        }

        protected override string Parse(out bool isFailed)
        {
            var s = this.input;
            isFailed = s != this.column.Text;
            return s;
        }
    }
}
