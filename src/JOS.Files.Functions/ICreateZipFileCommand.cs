using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JOS.Files.Functions;

public interface ICreateZipFileCommand
{
    Task Execute(string containerName, IReadOnlyCollection<string> filePaths, CancellationToken cancellationToken);
}