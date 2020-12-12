using System.Collections.Generic;
using System.Threading.Tasks;

namespace JOS.Files.Implementations
{
    public interface IZipFilesCommand
    {
        Task Execute(IReadOnlyCollection<string> filePaths);
    }
}
