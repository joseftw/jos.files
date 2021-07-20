using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using JOS.Files.Implementations.Sorting;

namespace JOS.Console.Runner
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var rows = 100_000_00;
            var unsortedFile = $"unsorted.{rows}.csv";
            var sortCommand = new ExternalMergeSortFileCommand();

            if (!File.Exists(Path.Combine(FileGenerator.FileLocation, unsortedFile)))
            {
                System.Console.WriteLine($"{unsortedFile} does not exists, creating...");
                await FileGenerator.CreateFile(rows, FileGenerator.FileLocation);
                System.Console.WriteLine($"{unsortedFile} has been created");
            }

            var filePath = Path.Combine(FileGenerator.FileLocation, unsortedFile);
            System.Console.WriteLine($"Starting to sort {unsortedFile}...");
            var stopwatch = Stopwatch.StartNew();
            await sortCommand.Execute(File.OpenRead(filePath));
            stopwatch.Stop();
            System.Console.WriteLine($"Done, took {stopwatch.Elapsed}");
        }
    }
}
