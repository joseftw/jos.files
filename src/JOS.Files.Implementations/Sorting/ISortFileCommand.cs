using System.IO;
using System.Threading.Tasks;

namespace JOS.Files.Implementations.Sorting
{
    public interface ISortFileCommand
    {
        Task Execute(Stream source, Stream target);
    }
}