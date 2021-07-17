using System;
using System.IO;
using System.Threading.Tasks;

namespace JOS.Files.Implementations.Sorting
{
    public class ExternalMergeSortFileCommand : ISortFileCommand
    {
        private const string UnsortedFileExtension = ".tmp.unsorted";
        public async Task Execute(Stream file)
        {
            await CreateUnsortedFiles(file);
            await SortTmpFiles();
        }

        private static async Task CreateUnsortedFiles(Stream file)
        {
            var fileSizeInBytes = file.Length;
            var fileSizeInMegaBytes = fileSizeInBytes / 1024 / 1024;
            var memoryLimitMegaBytes = 10M;
            var fileLimitInBytes = memoryLimitMegaBytes * 1024 * 1024;
            var runs = 9M;

            using var streamReader = new StreamReader(file);
            var run = 1;
            var bytesWritten = 0;

            var runPartial = 1;
            var runFile = File.Create(CreateUnsortedFilename(run, runPartial));
            var runWriter = new StreamWriter(runFile);

            while (!streamReader.EndOfStream)
            {
                if (bytesWritten < fileLimitInBytes)
                {
                    var line = await streamReader.ReadLineAsync();
                    await runWriter.WriteLineAsync(line);
                    bytesWritten += line.Length;
                }
                else
                {
                    await runWriter.FlushAsync();
                    await runWriter.DisposeAsync();
                    if (runPartial++ >= runs)
                    {
                        run++;
                        runPartial = 1;
                    }
                    runFile = File.Create(CreateUnsortedFilename(run, runPartial));
                    runWriter = new StreamWriter(runFile);
                    bytesWritten = 0;
                }
            }

            await runWriter.FlushAsync();
            await runWriter.DisposeAsync();
        }

        private static string CreateUnsortedFilename(int run, int runPartial)
        {
            return $"{run}_{runPartial}{UnsortedFileExtension}";
        }

        private static async Task SortTmpFiles()
        {
            var fileLocation = Directory.GetCurrentDirectory();
            foreach (var unsortedFile in Directory.EnumerateFiles(fileLocation, $"*{UnsortedFileExtension}"))
            {
                var allRows = await File.ReadAllLinesAsync(unsortedFile);
                Array.Sort(allRows);
                var sortedFilename = unsortedFile.Replace($"{UnsortedFileExtension}", ".sorted");
                await File.WriteAllLinesAsync(sortedFilename, allRows);
                File.Delete(unsortedFile);
            }
        }
    }
}
