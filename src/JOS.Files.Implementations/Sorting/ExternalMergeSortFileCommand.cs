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
        private const string SortedFileExtension = ".sorted";

        public ExternalMergeSortFileCommand() : this(new ExternalMergeSortOptions()) { }

        public ExternalMergeSortFileCommand(ExternalMergeSortOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task Execute(Stream source, Stream target)
        {
            var files = await SplitFile(source);

            if (files.Count == 1)
            {
                var unsortedFilePath = Path.Combine(_options.FileLocation, files.First());
                await SortFile(File.OpenRead(unsortedFilePath), target);
                //var rows = await File.ReadAllLinesAsync(Path.Combine(_options.FileLocation, files.First()));
                //Array.Sort(rows, _options.Sort.Comparer);
                //await using var streamWriter = new StreamWriter(target, bufferSize: 65536);
                //foreach (var row in rows)
                //{
                //    await streamWriter.WriteLineAsync(row);
                //}

                return;
            }

            var sortedFiles = await SortFiles(files);
            await MergeFiles(sortedFiles, target);
        }

        private async Task<IReadOnlyCollection<string>> SplitFile(
            Stream file,
            int? chunkSizeInBytes = null,
            char newLineSeparator = '\n',
            IProgress<int> progress = default)
        {
            var chunkSize = chunkSizeInBytes ?? _options.Split.ChunkSize;
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

        private async Task<IReadOnlyList<string>> SortFiles(IReadOnlyCollection<string> unsortedFiles)
        {
            var sortedFiles = new List<string>(unsortedFiles.Count);

            foreach (var unsortedFile in unsortedFiles)
            {
                //File.ReadAllLinesAsync()
                //var rows = await File.ReadAllLinesAsync(Path.Combine(_options.FileLocation, unsortedFile));
                //Array.Sort(rows, _options.Sort.Comparer);
                //var sortedFilename = unsortedFile.Replace(UnsortedFileExtension, SortedFileExtension);
                //await File.WriteAllLinesAsync(Path.Combine(_options.FileLocation, sortedFilename), rows);
                //File.Delete(Path.Combine(_options.FileLocation, unsortedFile));
                //sortedFiles.Add(sortedFilename);
                //await SortFile()
                var sortedFilename = unsortedFile.Replace(UnsortedFileExtension, SortedFileExtension);
                var unsortedFilePath = Path.Combine(_options.FileLocation, unsortedFile);
                var sortedFilePath = Path.Combine(_options.FileLocation, sortedFilename);
                await SortFile(File.OpenRead(unsortedFilePath), File.OpenWrite(sortedFilePath));
                File.Delete(unsortedFilePath);
                sortedFiles.Add(sortedFilename);
            }

            return sortedFiles;
        }

        private async Task SortFile(Stream unsortedFile, Stream target)
        {
            using var streamReader = new StreamReader(unsortedFile, bufferSize: 65536);
            var rows = new List<string>();
            while (!streamReader.EndOfStream)
            {
                rows.Add(await streamReader.ReadLineAsync());
            }

            rows.Sort((str1, str2) => _options.Sort.Comparer.Compare(str1, str2));
            await using var streamWriter = new StreamWriter(target, bufferSize: 65536);
            foreach (var row in rows)
            {
                await streamWriter.WriteLineAsync(row);
            }
        }

        private async Task MergeFiles(IReadOnlyList<string> sortedFiles, Stream target)
        {
            var chunkSize = _options.Merge.ChunkSize;
            var finalRun = sortedFiles.Count <= chunkSize;

            if (finalRun)
            {
                await MergeFile(sortedFiles, target);
                return;
            }

            var chunks = sortedFiles.Chunk(chunkSize);
            var chunkedCounter = 0;
            // TODO Handle chunks of one (last) better
            foreach (var files in chunks)
            {
                var outputFilename = $"{++chunkedCounter}{SortedFileExtension}.tmp";
                if (files.Length == 1)
                {
                    HandleSingleFile(files.First(), outputFilename);
                    continue;
                }

                var outputStream = File.OpenWrite(GetPath(outputFilename));
                await MergeFile(files, outputStream);
                File.Move(GetPath(outputFilename), GetPath(outputFilename.Replace(".tmp", string.Empty)), true);
            }

            sortedFiles = Directory.GetFiles(_options.FileLocation, "*.sorted").OrderBy(x =>
            {
                var filename = Path.GetFileNameWithoutExtension(x);
                return int.Parse(filename);
            }).ToArray();

            if (sortedFiles.Count > 1)
            {
                await MergeFiles(sortedFiles, target);
            }
        }

        private async Task MergeFile(IReadOnlyList<string> filesToMerge, Stream outputStream)
        {
            var (streamReaders, rows) = await GetStreamReaders(filesToMerge);
            var finishedStreamReaders = new List<int>();
            var done = false;
            await using var outputWriter = new StreamWriter(outputStream, bufferSize: _options.Merge.OutputBufferSize);

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

            for (var i = 0; i < streamReaders.Length; i++)
            {
                streamReaders[i].Dispose();
                // RENAME BEFORE DELETION SINCE DELETION OF LARGE FILES CAN TAKE SOME TIME
                // WE DONT WANT TO CLASH WHEN WRITING NEW FILES.
                var temporaryFilename = $"{filesToMerge[i]}.removal";
                File.Move(GetPath(filesToMerge[i]), GetPath(temporaryFilename));
                File.Delete(GetPath(temporaryFilename));
            }
        }

        private async Task<(StreamReader[] StreamReaders, List<Row> rows)> GetStreamReaders(IReadOnlyList<string> files)
        {
            var streamReaders = new StreamReader[files.Count];
            var rows = new List<Row>(files.Count);
            for (var i = 0; i < files.Count; i++)
            {
                var path = GetPath(files[i]);
                streamReaders[i] = new StreamReader(File.OpenRead(path), bufferSize: _options.Merge.InputBufferSize);
                var value = await streamReaders[i].ReadLineAsync();
                var row = new Row
                {
                    Value = value,
                    StreamReader = i
                };
                rows.Add(row);
            }

            return (streamReaders, rows);
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
        /// <summary>
        /// Size of unsorted file (chunk) (in bytes)
        /// </summary>
        public int ChunkSize { get; init; } = 2 * 1024 * 1024;
    }

    public class ExternalMergeSortSortOptions
    {
        public IComparer Comparer { get; init; } = Comparer<string>.Default;
    }

    public class ExternalMergeSortMergeOptions
    {
        public int ChunkSize { get; init; } = 10;
        /// <summary>
        /// Buffer size (in bytes) for input StreamReaders
        /// </summary>
        public int InputBufferSize { get; init; } = 65536;
        /// <summary>
        /// Buffer size (in bytes) for output StreamWriter
        /// </summary>
        public int OutputBufferSize { get; init; } = 65536;
    }

    internal readonly struct Row
    {
        public string Value { get; init; }
        public int StreamReader { get; init; }
    }
}
