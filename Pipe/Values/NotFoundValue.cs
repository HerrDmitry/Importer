namespace Importer.Pipe.Values
{
    using Importer.Pipe.Configuration;
    using Importer.Pipe.Parsers;
    public struct NotFoundValue:IValue
    {
        public string ToString(string format, string nullValue = "")
        {
            return nullValue;
        }

        public bool IsNull => false;

        public bool IsFailed => true;

        public Column Column { get; }
    }
}