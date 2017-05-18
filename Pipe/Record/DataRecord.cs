namespace Importer.Pipe.Record
{
    using System.Collections.Generic;

    using Importer.Pipe.Parsers;

    public struct DataRecord
    {
        public IEnumerable<string> Source;

        public Dictionary<string, IValue> Parsed;
    }
}