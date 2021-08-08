using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JOS.Files.Implementations.Sorting
{
    public class InMemorySortCommand : ISortFileCommand
    {
        public async Task Execute(Stream source, Stream target, CancellationToken cancellationToken = default)
        {
            using var streamReader = new StreamReader(source);
            var lines = new List<string>();
            while (!streamReader.EndOfStream)
            {
                lines.Add((await streamReader.ReadLineAsync())!);
            }

            await using var streamWriter = new StreamWriter(target);
            foreach (var line in lines.OrderBy(x => x))
            {
                await streamWriter.WriteLineAsync(line);
            }
        }
    }
}
