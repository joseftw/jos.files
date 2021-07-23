using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JOS.ExternalMergeSort
{
    public class ExternalMergeSorter
    {
        private readonly ExternalMergeSorterOptions _options;
        private const string UnsortedFileExtension = ".unsorted";
        private const string SortedFileExtension = ".sorted";
        private const string TempFileExtension = ".tmp";

        public ExternalMergeSorter() : this(new ExternalMergeSorterOptions()) { }

        public ExternalMergeSorter(ExternalMergeSorterOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task Sort(Stream source, Stream target, CancellationToken cancellationToken)
        {
            var files = await SplitFile(source, cancellationToken);

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
            await MergeFiles(sortedFiles, target, cancellationToken);
        }

        private async Task<IReadOnlyCollection<string>> SplitFile(
            Stream sourceStream,
            CancellationToken cancellationToken,
            IProgress<int> progress = default)
        {
            var chunkSize = _options.Split.ChunkSize;
            var buffer = new byte[chunkSize];
            var extraBuffer = new List<byte>();
            var filenames = new List<string>();
            await using (sourceStream)
            {
                var index = 0;
                while (sourceStream.Position < sourceStream.Length)
                {
                    var chunkBytesRead = 0;
                    while (chunkBytesRead < chunkSize)
                    {
                        var bytesRead = await sourceStream.ReadAsync(
                            buffer.AsMemory(chunkBytesRead, chunkSize - chunkBytesRead),
                            cancellationToken);

                        if (bytesRead == 0)
                        {
                            break;
                        }

                        chunkBytesRead += bytesRead;
                    }

                    var extraByte = buffer[chunkSize - 1];

                    while (extraByte != _options.Split.NewLineSeparator)
                    {
                        var flag = sourceStream.ReadByte();
                        if (flag == -1)
                        {
                            break;
                        }
                        extraByte = (byte)flag;
                        extraBuffer.Add(extraByte);
                    }

                    var filename = $"{++index}.unsorted";
                    await using var unsortedFile = File.Create(Path.Combine(_options.FileLocation, filename));
                    await unsortedFile.WriteAsync(buffer, 0, chunkBytesRead, cancellationToken);
                    if (extraBuffer.Count > 0)
                    {
                        await unsortedFile.WriteAsync(extraBuffer.ToArray(), 0, extraBuffer.Count, cancellationToken);
                    }
                    filenames.Add(filename);
                    extraBuffer.Clear();
                }

                return filenames;
            }
        }

        private async Task<IReadOnlyList<string>> SortFiles(
            IReadOnlyCollection<string> unsortedFiles)
        {
            var sortedFiles = new List<string>(unsortedFiles.Count);

            foreach (var unsortedFile in unsortedFiles)
            {
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
            using var streamReader = new StreamReader(unsortedFile, bufferSize: _options.Sort.InputBufferSize);
            var rows = new List<string>();
            while (!streamReader.EndOfStream)
            {
                rows.Add(await streamReader.ReadLineAsync());
            }

            rows.Sort((str1, str2) => _options.Sort.Comparer.Compare(str1, str2));
            await using var streamWriter = new StreamWriter(target, bufferSize: _options.Sort.OutputBufferSize);
            foreach (var row in rows)
            {
                await streamWriter.WriteLineAsync(row);
            }
        }

        private async Task MergeFiles(
            IReadOnlyList<string> sortedFiles,
            Stream target,
            CancellationToken cancellationToken)
        {
            var chunkSize = _options.Merge.ChunkSize;
            var finalRun = sortedFiles.Count <= chunkSize;

            if (finalRun)
            {
                await Merge(sortedFiles, target, cancellationToken);
                return;
            }

            var chunks = sortedFiles.Chunk(chunkSize);
            var chunkCounter = 0;
            // TODO Handle chunks of one (last) better
            foreach (var files in chunks)
            {
                var outputFilename = $"{++chunkCounter}{SortedFileExtension}{TempFileExtension}";
                if (files.Length == 1)
                {
                    File.Move(GetPath(files.First()), GetPath(outputFilename.Replace(TempFileExtension, string.Empty)));
                    continue;
                }

                var outputStream = File.OpenWrite(GetPath(outputFilename));
                await Merge(files, outputStream, cancellationToken);
                File.Move(GetPath(outputFilename), GetPath(outputFilename.Replace(TempFileExtension, string.Empty)), true);
            }

            sortedFiles = Directory.GetFiles(_options.FileLocation, $"*{SortedFileExtension}").OrderBy(x =>
            {
                var filename = Path.GetFileNameWithoutExtension(x);
                return int.Parse(filename);
            }).ToArray();

            if (sortedFiles.Count > 1)
            {
                await MergeFiles(sortedFiles, target, cancellationToken);
            }
        }

        private async Task Merge(
            IReadOnlyList<string> filesToMerge,
            Stream outputStream,
            CancellationToken cancellationToken)
        {
            var (streamReaders, rows) = await InitializeStreamReaders(filesToMerge);
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

        /// <summary>
        /// Creates a StreamReader for each sorted sourceStream.
        /// Reads one line per StreamReader to initialize the rows list.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private async Task<(StreamReader[] StreamReaders, List<Row> rows)> InitializeStreamReaders(
            IReadOnlyList<string> files)
        {
            var streamReaders = new StreamReader[files.Count];
            var rows = new List<Row>(files.Count);
            for (var i = 0; i < files.Count; i++)
            {
                var sortedFilePath = GetPath(files[i]);
                var sortedFileStream = File.OpenRead(sortedFilePath);
                streamReaders[i] = new StreamReader(sortedFileStream, bufferSize: _options.Merge.InputBufferSize);
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
    }
}
