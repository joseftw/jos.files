using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JOS.Files.Implementations.Sorting
{
    public class ExternalMergeSortFileCommand : ISortFileCommand
    {
        private const string FileLocation = "c:\\temp\\files";
        private const string UnsortedFileExtension = ".unsorted";
        public async Task Execute(Stream file)
        {
            var files = await SplitFile(file, 16);
            var sortedFiles = await SortFiles(files);
            var mergedFiles = await MergeFiles(sortedFiles);
            //var sortedFile = await SortFile(mergedFiles);
        }

        public static async Task<IReadOnlyCollection<string>> SplitFile(Stream file, int chunkSizeMb, char newLineSeparator = '\n')
        {
            var chunkSize = chunkSizeMb * 1024 * 1024;
            var buffer = new byte[chunkSize];
            var extraBuffer = new List<byte>();
            var filenames = new List<string>();
            await using (file)
            {
                var index = 0;
                while (file.Position < file.Length)
                {
                    var chunkBytesRead = 0;
                    while (chunkBytesRead < chunkSize)
                    {
                        var bytesRead = await file.ReadAsync(buffer,
                            chunkBytesRead,
                            chunkSize - chunkBytesRead);

                        if (bytesRead == 0)
                        {
                            break;
                        }

                        chunkBytesRead += bytesRead;
                    }

                    var extraByte = buffer[chunkSize - 1];

                    // TODO IMPROVE, if extra bytes, append to last string in buffer.
                    // THEN SORT HERE.
                    while (extraByte != newLineSeparator)
                    {
                        var flag = file.ReadByte();
                        if (flag == -1)
                        {
                            break;
                        }
                        extraByte = (byte)flag;
                        extraBuffer.Add(extraByte);
                    }

                    var filename = $"{++index}.unsorted";
                    await using var unsortedFile = File.Create(Path.Combine(FileLocation, filename));
                    await unsortedFile.WriteAsync(buffer, 0, chunkBytesRead);
                    if (extraBuffer.Count > 0)
                    {
                        await unsortedFile.WriteAsync(extraBuffer.ToArray(), 0, extraBuffer.Count);
                    }
                    filenames.Add(filename);
                    Console.WriteLine($"Created {filename}...");

                    extraBuffer.Clear();
                }

                return filenames;
            }
        }

        private static async Task<IReadOnlyCollection<string>> SortFiles(IReadOnlyCollection<string> files)
        {
            var sortedFiles = new List<string>(files.Count);
            foreach (var filename in files)
            {
                var rows = await File.ReadAllLinesAsync(Path.Combine(FileLocation, filename));
                Array.Sort(rows);
                var sortedFilename = filename.Replace(UnsortedFileExtension, ".sorted");
                await File.WriteAllLinesAsync(Path.Combine(FileLocation, sortedFilename), rows);
                Console.WriteLine($"Created {sortedFilename}...");
                File.Delete(Path.Combine(FileLocation, filename));
                sortedFiles.Add(sortedFilename);
                rows = null;
                GC.Collect();
            }

            return sortedFiles;
        }

        private static async Task<IReadOnlyCollection<string>> MergeFiles(IReadOnlyCollection<string> sortedFiles)
        {
            var chunkSize = 10;
            var chunks = sortedFiles.Chunk(chunkSize).ToList();
            var chunkedCounter = 0;
            foreach (var files in chunks)
            {
                var streamReaders = new StreamReader[files.Length];
                var rows = new List<(string Value, int StreamReader)>(files.Length);
                for (var i = 0; i < files.Length; i++)
                {
                    var path = GetPath(files[i]);
                    streamReaders[i] = new StreamReader(File.OpenRead(path), bufferSize: 65536 / 2);
                    var value = await streamReaders[i].ReadLineAsync();
                    rows.Add((value, i));
                }

                var outputFilename = $"{++chunkedCounter}.sorted.tmp";
                var chunkedOutputFile = File.OpenWrite(GetPath(outputFilename));
                await using (var outputWriter = new StreamWriter(chunkedOutputFile, bufferSize: 65536))
                {
                    while (!streamReaders.All(x => x.EndOfStream))
                    {
                        rows.Sort();
                        var valueToWrite = rows[0].Value;
                        var streamReaderIndex = rows[0].StreamReader;
                        await outputWriter.WriteLineAsync(valueToWrite);

                        var nextStreamReader = !streamReaders[streamReaderIndex].EndOfStream
                            ? streamReaders[streamReaderIndex]
                            : streamReaders.First(x => !x.EndOfStream);

                        if (streamReaders[streamReaderIndex].EndOfStream)
                        {
                            var toRemove = rows.FindIndex(x => x.StreamReader == streamReaderIndex);
                            rows.RemoveAt(toRemove);
                            continue;
                        }

                        var value = await nextStreamReader.ReadLineAsync();
                        rows[0] = (value, streamReaderIndex);
                    }
                }
                
                foreach (var streamReader in streamReaders)
                {
                    streamReader.Dispose();
                }

                foreach (var file in files)
                {
                    var path = GetPath(file);
                    File.Delete(path);
                }

                File.Move(GetPath(outputFilename), GetPath(outputFilename.Replace(".tmp", string.Empty)));
            }

            var chunkedFiles = Directory.GetFiles(FileLocation, "*.sorted");
            if (chunkedFiles.Length > 1)
            {
                await MergeFiles(chunkedFiles);
            }

            return chunkedFiles;
        }

        private static async Task<string> SortFile(IReadOnlyCollection<string> mergedFiles)
        {
            return string.Empty;
        }

        private static string GetPath(string filename)
        {
            return Path.Combine(FileLocation, filename);
        }
    }
}
