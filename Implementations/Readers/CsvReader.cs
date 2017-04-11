using System;
using System.Collections.Generic;
using System.IO;
using Importer.Interfaces;

namespace Importer.Implementations.Readers
{
    public class CsvReader : ReaderBase,IReader
    {
        public CsvReader(Stream source)
        {
            this.source = new StreamReader(source);
        }

        public IEnumerable<IInputRecord> Read()
        {
            throw new NotImplementedException();
        }

        public IInputRecord ReadNext()
        {
            if (!source.EndOfStream)
            {
                var line = source.ReadLine();
            }

            return null;
        }

        private StreamReader source;
    }
}
