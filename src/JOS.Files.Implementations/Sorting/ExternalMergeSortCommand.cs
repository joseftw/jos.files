using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JOS.Files.Implementations.Sorting
{
    public class ExternalMergeSortCommand : ISortFileCommand
    {
        public async Task Execute(Stream source, Stream target, CancellationToken cancellationToken = default)
        {
            await new ExternalMergeSort.ExternalMergeSort().Sort(source, target, cancellationToken);
        }
    }
}
