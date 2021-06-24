using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JOS.Files.Implementations.Sorting
{
    public class InMemorySortFileCommand : ISortFileCommand
    {
        public async Task Execute(Stream file)
        {
            using var streamReader = new StreamReader(file);
            var lines = new List<string>();
            while (!streamReader.EndOfStream)
            {
                lines.Add(await streamReader.ReadLineAsync());
            }

            await using var sortedFile = File.OpenWrite("sorted.txt");
            await using var streamWriter = new StreamWriter(sortedFile);
            foreach (var line in lines.OrderBy(x => x))
            {
                await streamWriter.WriteLineAsync(line);
            }
        }
    }
}
