using System.IO;

namespace Importer.Interfaces
{
    public interface IWriter
    {
        void SetDataDestination(Stream stream);

        void Write(IRecord record);

        void Flush();
    }
}
