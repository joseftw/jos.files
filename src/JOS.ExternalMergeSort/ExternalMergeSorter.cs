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
        private long _maxUnsortedRows;
        private string[] _unsortedRows;
        private double _totalFilesToMerge;
        private int _mergeFilesProcessed;
        private readonly ExternalMergeSorterOptions _options;
        private const string UnsortedFileExtension = ".unsorted";
        private const string SortedFileExtension = ".sorted";
        private const string TempFileExtension = ".tmp";

        public ExternalMergeSorter() : this(new ExternalMergeSorterOptions()) { }

        public ExternalMergeSorter(ExternalMergeSorterOptions options)
        {
            _totalFilesToMerge = 0;
            _mergeFilesProcessed = 0;
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _unsortedRows = Array.Empty<string>();
        }

        public async Task Sort(Stream source, Stream target, CancellationToken cancellationToken)
        {
            var files = await SplitFile(source, cancellationToken);
            _unsortedRows = new string[_maxUnsortedRows];
            if (files.Count == 1)
            {
                var unsortedFilePath = Path.Combine(_options.FileLocation, files.First());
                await SortFile(File.OpenRead(unsortedFilePath), target);
                return;
            }
            var sortedFiles = await SortFiles(files);

            var done = false;
            var size = _options.Merge.FilesPerRun;
            _totalFilesToMerge = sortedFiles.Count;
            var result = sortedFiles.Count / size;

            while (!done)
            {
                if (result <= 0)
                {
                    done = true;
                }
                _totalFilesToMerge += result;
                result /= size;
            }

            await MergeFiles(sortedFiles, target, cancellationToken);
        }

        private async Task<IReadOnlyCollection<string>> SplitFile(
            Stream sourceStream,
            CancellationToken cancellationToken)
        {
            var fileSize = _options.Split.FileSize;
            var buffer = new byte[fileSize];
            var extraBuffer = new List<byte>();
            var filenames = new List<string>();
            var totalFiles = Math.Ceiling(sourceStream.Length / (double)_options.Split.FileSize);

            await using (sourceStream)
            {
                var currentFile = 0L;
                while (sourceStream.Position < sourceStream.Length)
                {
                    var totalRows = 0;
                    var runBytesRead = 0;
                    while (runBytesRead < fileSize)
                    {
                        var value = sourceStream.ReadByte();
                        if (value == -1)
                        {
                            break;
                        }

                        var @byte = (byte)value;
                        buffer[runBytesRead] = @byte;
                        runBytesRead++;
                        if (@byte == _options.Split.NewLineSeparator)
                        {
                            totalRows++;
                        }
                    }

                    var extraByte = buffer[fileSize - 1];

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

                    var filename = $"{++currentFile}.unsorted";
                    await using var unsortedFile = File.Create(Path.Combine(_options.FileLocation, filename));
                    await unsortedFile.WriteAsync(buffer.AsMemory(0, runBytesRead), cancellationToken);
                    if (extraBuffer.Count > 0)
                    {
                        totalRows++;
                        await unsortedFile.WriteAsync(extraBuffer.ToArray(), 0, extraBuffer.Count, cancellationToken);
                    }

                    if (totalRows > _maxUnsortedRows)
                    {
                        _maxUnsortedRows = totalRows;
                    }

                    _options.Split.ProgressHandler?.Report(currentFile / totalFiles);
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
            double totalFiles = unsortedFiles.Count;
            foreach (var unsortedFile in unsortedFiles)
            {
                var sortedFilename = unsortedFile.Replace(UnsortedFileExtension, SortedFileExtension);
                var unsortedFilePath = Path.Combine(_options.FileLocation, unsortedFile);
                var sortedFilePath = Path.Combine(_options.FileLocation, sortedFilename);
                await SortFile(File.OpenRead(unsortedFilePath), File.OpenWrite(sortedFilePath));
                File.Delete(unsortedFilePath);
                sortedFiles.Add(sortedFilename);
                _options.Sort.ProgressHandler?.Report(sortedFiles.Count / totalFiles);
            }
            return sortedFiles;
        }

        private async Task SortFile(Stream unsortedFile, Stream target)
        {
            using var streamReader = new StreamReader(unsortedFile, bufferSize: _options.Sort.InputBufferSize);
            var counter = 0;
            while (!streamReader.EndOfStream)
            {
                _unsortedRows[counter++] = (await streamReader.ReadLineAsync())!;
            }

            Array.Sort(_unsortedRows, _options.Sort.Comparer);
            await using var streamWriter = new StreamWriter(target, bufferSize: _options.Sort.OutputBufferSize);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            foreach (var row in _unsortedRows.Where(x => x is not null))
            {
                await streamWriter.WriteLineAsync(row);
            }

            Array.Clear(_unsortedRows, 0, _unsortedRows.Length);
        }

        private async Task MergeFiles(
            IReadOnlyList<string> sortedFiles, Stream target, CancellationToken cancellationToken)
        {
            var done = false;
            while (!done)
            {
                var runSize = _options.Merge.FilesPerRun;
                var finalRun = sortedFiles.Count <= runSize;

                if (finalRun)
                {
                    await Merge(sortedFiles, target, cancellationToken);
                    return;
                }

                // TODO better logic when chunking
                // we don't want to have 1 chunk of 10 and 1 of 1 for example, better to spread it out.
                var runs = sortedFiles.Chunk(runSize);
                var chunkCounter = 0;
                foreach (var files in runs)
                {
                    var outputFilename = $"{++chunkCounter}{SortedFileExtension}{TempFileExtension}";
                    if (files.Length == 1)
                    {
                        OverwriteTempFile(files.First(), outputFilename);
                        continue;
                    }

                    var outputStream = File.OpenWrite(GetFullPath(outputFilename));
                    await Merge(files, outputStream, cancellationToken);
                    OverwriteTempFile(outputFilename, outputFilename);
                    
                    void OverwriteTempFile(string from, string to)
                    {
                        File.Move(
                            GetFullPath(from),
                            GetFullPath(to.Replace(TempFileExtension, string.Empty)), true);
                    }
                }

                sortedFiles = Directory.GetFiles(_options.FileLocation, $"*{SortedFileExtension}")
                    .OrderBy(x =>
                    {
                        var filename = Path.GetFileNameWithoutExtension(x);
                        return int.Parse(filename);
                    })
                    .ToArray();

                if (sortedFiles.Count > 1)
                {
                    continue;
                }

                done = true;
            }
        }

        private async Task Merge(
            IReadOnlyList<string> filesToMerge,
            Stream outputStream,
            CancellationToken cancellationToken)
        {
            var (streamReaders, rows) = await InitializeStreamReaders(filesToMerge);
            var finishedStreamReaders = new List<int>(streamReaders.Length);
            var done = false;
            await using var outputWriter = new StreamWriter(outputStream, bufferSize: _options.Merge.OutputBufferSize);

            while (!done)
            {
                rows.Sort((row1, row2) => _options.Sort.Comparer.Compare(row1.Value, row2.Value));
                var valueToWrite = rows[0].Value;
                var streamReaderIndex = rows[0].StreamReader;
                await outputWriter.WriteLineAsync(valueToWrite.AsMemory(), cancellationToken);

                if (streamReaders[streamReaderIndex].EndOfStream)
                {
                    var indexToRemove = rows.FindIndex(x => x.StreamReader == streamReaderIndex);
                    rows.RemoveAt(indexToRemove);
                    finishedStreamReaders.Add(streamReaderIndex);
                    done = finishedStreamReaders.Count == streamReaders.Length;
                    _options.Merge.ProgressHandler?.Report(++_mergeFilesProcessed / _totalFilesToMerge);
                    continue;
                }

                var value = await streamReaders[streamReaderIndex].ReadLineAsync(cancellationToken);
                rows[0] = new Row { Value = value!, StreamReader = streamReaderIndex };
            }

            CleanupRun(streamReaders, filesToMerge);
        }

        /// <summary>
        /// Creates a StreamReader for each sorted sourceStream.
        /// Reads one line per StreamReader to initialize the rows list.
        /// </summary>
        /// <param name="sortedFiles"></param>
        /// <returns></returns>
        private async Task<(StreamReader[] StreamReaders, List<Row> rows)> InitializeStreamReaders(
            IReadOnlyList<string> sortedFiles)
        {
            var streamReaders = new StreamReader[sortedFiles.Count];
            var rows = new List<Row>(sortedFiles.Count);
            for (var i = 0; i < sortedFiles.Count; i++)
            {
                var sortedFilePath = GetFullPath(sortedFiles[i]);
                var sortedFileStream = File.OpenRead(sortedFilePath);
                streamReaders[i] = new StreamReader(sortedFileStream, bufferSize: _options.Merge.InputBufferSize);
                var value = await streamReaders[i].ReadLineAsync();
                var row = new Row
                {
                    Value = value!,
                    StreamReader = i
                };
                rows.Add(row);
            }

            return (streamReaders, rows);
        }

        /// <summary>
        /// Disposes all StreamReaders
        /// Renames old files to a temporary name and then deletes them.
        /// Reason for renaming first is that large files can take quite some time to remove
        /// and the .Delete call returns immediately.
        /// </summary>
        /// <param name="streamReaders"></param>
        /// <param name="filesToMerge"></param>
        private void CleanupRun(StreamReader[] streamReaders, IReadOnlyList<string> filesToMerge)
        {
            for (var i = 0; i < streamReaders.Length; i++)
            {
                streamReaders[i].Dispose();
                // RENAME BEFORE DELETION SINCE DELETION OF LARGE FILES CAN TAKE SOME TIME
                // WE DON'T WANT TO CLASH WHEN WRITING NEW FILES.
                var temporaryFilename = $"{filesToMerge[i]}.removal";
                File.Move(GetFullPath(filesToMerge[i]), GetFullPath(temporaryFilename));
                File.Delete(GetFullPath(temporaryFilename));
            }
        }

        private string GetFullPath(string filename)
        {
            return Path.Combine(_options.FileLocation, Path.GetFileName(filename));
        }
    }
}
