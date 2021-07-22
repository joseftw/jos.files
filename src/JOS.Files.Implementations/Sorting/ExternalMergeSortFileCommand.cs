using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JOS.Files.Implementations.Sorting
{
    public class ExternalMergeSortFileCommand : ISortFileCommand
    {
        private readonly ExternalMergeSortOptions _options;
        private const string UnsortedFileExtension = ".unsorted";

        public ExternalMergeSortFileCommand() : this(new ExternalMergeSortOptions()) {}

        public ExternalMergeSortFileCommand(ExternalMergeSortOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task Execute(Stream source, Stream target)
        {
            var files = await SplitFile(source);
            var sortedFiles = await SortFiles(files);
            _ = await MergeFiles(sortedFiles);
            // TEMPORARY
            await target.DisposeAsync();
        }

        private async Task<IReadOnlyCollection<string>> SplitFile(Stream file, int? chunkSizeMb = null, char newLineSeparator = '\n')
        {
            var chunkSize = (chunkSizeMb ?? _options.Split.ChunkSizeMb) * 1024 * 1024;
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
                    await using var unsortedFile = File.Create(Path.Combine(_options.FileLocation, filename));
                    await unsortedFile.WriteAsync(buffer, 0, chunkBytesRead);
                    if (extraBuffer.Count > 0)
                    {
                        await unsortedFile.WriteAsync(extraBuffer.ToArray(), 0, extraBuffer.Count);
                    }
                    filenames.Add(filename);

                    extraBuffer.Clear();
                }

                return filenames;
            }
        }

        private async Task<IReadOnlyCollection<string>> SortFiles(IReadOnlyCollection<string> files)
        {
            var sortedFiles = new List<string>(files.Count);
            foreach (var filename in files)
            {
                var rows = await File.ReadAllLinesAsync(Path.Combine(_options.FileLocation, filename));
                Array.Sort(rows, _options.Sort.Comparer);
                var sortedFilename = filename.Replace(UnsortedFileExtension, ".sorted");
                await File.WriteAllLinesAsync(Path.Combine(_options.FileLocation, sortedFilename), rows);
                File.Delete(Path.Combine(_options.FileLocation, filename));
                sortedFiles.Add(sortedFilename);
            }

            return sortedFiles;
        }

        private async Task<string> MergeFiles(IReadOnlyCollection<string> sourceFiles)
        {
            var chunkSize = _options.Merge.ChunkSize;
            var chunks = sourceFiles.Chunk(chunkSize);
            var chunkedCounter = 0;
            foreach (var files in chunks)
            {
                var outputFilename = $"{++chunkedCounter}.sorted.tmp";
                if (files.Length == 1)
                {
                    Console.WriteLine("ONE FILE");
                    HandleSingleFile(files.First(), outputFilename);
                    continue;
                }

                var streamReaders = new StreamReader[files.Length];
                var rows = new List<Row>(files.Length);
                for (var i = 0; i < files.Length; i++)
                {
                    var path = GetPath(files[i]);
                    streamReaders[i] = new StreamReader(File.OpenRead(path), bufferSize: 65536);
                    var value = await streamReaders[i].ReadLineAsync();
                    var row = new Row
                    {
                        Value = value,
                        StreamReader = i
                    };
                    rows.Add(row);
                }

                var outputStream = File.OpenWrite(GetPath(outputFilename));
                var finishedStreamReaders = new List<int>();
                var done = false;
                await using (var outputWriter = new StreamWriter(outputStream, bufferSize: 65536))
                {
                    while (!done)
                    {
                        rows.Sort((row1, row2) => _options.Sort.Comparer.Compare(row1.Value, row2.Value));
                        var valueToWrite = rows[0].Value;
                        var streamReaderIndex = rows[0].StreamReader;
                        await outputWriter.WriteLineAsync(valueToWrite);

                        if (streamReaders[streamReaderIndex].EndOfStream)
                        {
                            var indexToRemove = rows.FindIndex(x => x.StreamReader == streamReaderIndex);
                            rows.RemoveAt(indexToRemove);
                            finishedStreamReaders.Add(streamReaderIndex);
                            done = finishedStreamReaders.Count == streamReaders.Length;
                            continue;
                        }

                        var value = await streamReaders[streamReaderIndex].ReadLineAsync();
                        rows[0] = new Row { Value = value, StreamReader = streamReaderIndex };
                    }

                    await outputWriter.FlushAsync();
                }

                for (var i = 0; i < streamReaders.Length; i++)
                {
                    streamReaders[i].Dispose();
                    // RENAME BEFORE DELETION SINCE DELETION OF LARGE FILES CAN TAKE SOME TIME
                    // WE DONT WANT TO CLASH WHEN WRITING NEW FILES.
                    var temporaryFilename = $"{files[i]}.removal";
                    File.Move(GetPath(files[i]), GetPath(temporaryFilename));
                    File.Delete(GetPath(temporaryFilename));
                }

                File.Move(GetPath(outputFilename), GetPath(outputFilename.Replace(".tmp", string.Empty)), true);
            }

            sourceFiles = Directory.GetFiles(_options.FileLocation, "*.sorted").OrderBy(x =>
            {
                var filename = Path.GetFileNameWithoutExtension(x);
                return int.Parse(filename);
            }).ToList();

            if (sourceFiles.Count > 1)
            {
                await MergeFiles(sourceFiles);
            }

            return sourceFiles.First();
        }

        private string GetPath(string filename)
        {
            return Path.Combine(_options.FileLocation, filename);
        }

        private void HandleSingleFile(string filename, string outputFilename)
        {
            File.Move(GetPath(filename), GetPath(outputFilename.Replace(".tmp", string.Empty)));
        }
    }

    public class ExternalMergeSortOptions
    {
        public ExternalMergeSortOptions()
        {
            Split = new ExternalMergeSortSplitOptions();
            Sort = new ExternalMergeSortSortOptions();
            Merge = new ExternalMergeSortMergeOptions();
        }

        public string FileLocation { get; init; } = "c:\\temp\\files";
        public ExternalMergeSortSplitOptions Split { get; init; }
        public ExternalMergeSortSortOptions Sort { get; init; }
        public ExternalMergeSortMergeOptions Merge { get; init; }
    }

    public class ExternalMergeSortSplitOptions
    {
        public int ChunkSizeMb { get; init; } = 1;
    }

    public class ExternalMergeSortSortOptions
    {
        public IComparer Comparer { get; init; } = Comparer<string>.Default;
    }

    public class ExternalMergeSortMergeOptions
    {
        public int ChunkSize { get; init; } = 10;
    }

    internal readonly struct Row
    {
        public string Value { get; init; }
        public int StreamReader { get; init; }
    }
}
