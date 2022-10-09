using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JOS.ExternalMergeSort;

namespace JOS.Files.Implementations.Sorting
{
    public class ExternalMergeSortCommand : ISortFileCommand
    {
        public async Task Execute(Stream source, Stream target, CancellationToken cancellationToken = default)
        {
            await new ExternalMergeSorter().Sort(source, target, cancellationToken);
        }
    }
}
