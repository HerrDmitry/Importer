using System.IO;
using System.Threading.Tasks;

namespace Importer.Interfaces
{
    public interface IWriter
    {
        void SetDataDestination(Stream stream, Stream errorOutputStream = null);

        Task WriteAsync(IRecord record);

        void Write(IRecord record);

        Task FlushAsync();

        void Close();
    }
}
