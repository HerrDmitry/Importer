using System.IO;
using System.Threading.Tasks;
using Importer.Writers;

namespace Importer.Interfaces
{
    public interface IWriter
    {
        void SetDataDestination(Stream stream, FileWriter errorOutputStream = null);

        Task WriteAsync(IRecord record);

        void Write(IRecord record);

        Task FlushAsync();

        void Close();
    }
}
