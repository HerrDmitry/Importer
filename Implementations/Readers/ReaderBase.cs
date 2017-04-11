using System;
using System.IO;

namespace Importer.Implementations.Readers
{
    public class ReaderBase
    {
        protected ReaderBase(){}
        public ReaderBase(Stream source){
            throw new NotImplementedException();
        }
    }
}
