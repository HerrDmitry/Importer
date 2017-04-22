using System;
using System.IO;
using Importer.Interfaces;

namespace Writers
{
    public class CsvWriter:IWriter
    {
        public CsvWriter()
        {
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public void SetDataDestination(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void Write(IOutputRecord record)
        {
            ///TODO: do something
        }
    }
}
