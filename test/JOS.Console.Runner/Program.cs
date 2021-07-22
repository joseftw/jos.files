using System;
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
            var rows = 1_000_000;
            var sourceFilename = $"unsorted.{rows}.csv";
            var sortCommand = new ExternalMergeSortFileCommand();
            var unsortedFilePath = Path.Combine(FileGenerator.FileLocation, sourceFilename);
            if (!File.Exists(unsortedFilePath))
            {
                System.Console.WriteLine($"{sourceFilename} does not exists, creating...");
                await FileGenerator.CreateFile(rows, FileGenerator.FileLocation);
                System.Console.WriteLine($"{sourceFilename} has been created");
            }

            var sourceFile = Path.Combine(FileGenerator.FileLocation, sourceFilename);
            var targetFile = File.OpenWrite(Path.Combine(FileGenerator.FileLocation, $"sorted.{rows}.csv"));
            System.Console.WriteLine($"Starting to sort {sourceFilename}...");
            var stopwatch = Stopwatch.StartNew();
            await sortCommand.Execute(File.OpenRead(sourceFile), targetFile);
            stopwatch.Stop();
            System.Console.WriteLine($"MergeSort done, took {stopwatch.Elapsed}");

            System.Console.WriteLine("Starting to sort In-memory...");
            stopwatch.Restart();
            var unsortedRows = await File.ReadAllLinesAsync(unsortedFilePath);
            Array.Sort(unsortedRows);
            await File.WriteAllLinesAsync(Path.Combine(FileGenerator.FileLocation, "sorted.inmemory.csv"), unsortedRows);
            stopwatch.Stop();
            System.Console.WriteLine($"In-memory done, took {stopwatch.Elapsed}");
        }
    }
}
