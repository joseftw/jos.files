using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JOS.ExternalMergeSort;
using JOS.Files.Implementations.Sorting;

namespace JOS.Console.Runner
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {

            var rows = 100_000_000;
            var sourceFilename = $"unsorted.{rows}.csv";
            var unsortedFilePath = Path.Combine(FileGenerator.FileLocation, sourceFilename);
            if (!File.Exists(unsortedFilePath))
            {
                System.Console.WriteLine($"{sourceFilename} does not exists, creating...");
                await FileGenerator.CreateFile(rows, FileGenerator.FileLocation);
                System.Console.WriteLine($"{sourceFilename} has been created");
            }

            var splitFileProgressHandler = new Progress<double>(x =>
            {
                var percentage = x * 100;
                System.Console.WriteLine($"Split progress: {percentage:##.##}%");
            });
            var sortFilesProgressHandler = new Progress<double>(x =>
            {
                var percentage = x * 100;
                System.Console.WriteLine($"Sort progress: {percentage:##.##}%");
            });
            var mergeFilesProgressHandler = new Progress<double>(x =>
            {
                var percentage = x * 100;
                System.Console.WriteLine($"Merge progress: {percentage:##.##}%");
            });

            var sortCommand = new ExternalMergeSorter(new ExternalMergeSorterOptions
            {
                Split = new ExternalMergeSortSplitOptions
                {
                    ProgressHandler = splitFileProgressHandler
                },
                Sort = new ExternalMergeSortSortOptions
                {
                    ProgressHandler = sortFilesProgressHandler
                },
                Merge = new ExternalMergeSortMergeOptions
                {
                    ProgressHandler = mergeFilesProgressHandler
                }
            });

            var sourceFile = Path.Combine(FileGenerator.FileLocation, sourceFilename);
            var targetFile = File.OpenWrite(Path.Combine(FileGenerator.FileLocation, $"sorted.{rows}.csv"));
            System.Console.WriteLine($"Starting to sort {sourceFilename}...");
            var stopwatch = Stopwatch.StartNew();
            await sortCommand.Sort(File.OpenRead(sourceFile), targetFile, CancellationToken.None);
            stopwatch.Stop();
            System.Console.WriteLine($"MergeSort done, took {stopwatch.Elapsed}");

            //System.Console.WriteLine("Starting to sort In-memory...");
            //stopwatch.Restart();
            //var unsortedRows = await File.ReadAllLinesAsync(unsortedFilePath);
            //Array.Sort(unsortedRows);
            //await File.WriteAllLinesAsync(Path.Combine(FileGenerator.FileLocation, "sorted.inmemory.csv"), unsortedRows);
            //stopwatch.Stop();
            //System.Console.WriteLine($"In-memory done, took {stopwatch.Elapsed}");
        }
    }
}
