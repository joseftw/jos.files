using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace JOS.ExternalMergeSort.IntegrationTests;

[Collection("LargeFiles")]
public class ExternalMergeSort_LargeFiles_IntegrationTests : IClassFixture<LargeFilesFixture>
{
    private readonly LargeFilesFixture _fixture;
    private readonly ExternalMergeSorter _sut;

    public ExternalMergeSort_LargeFiles_IntegrationTests(LargeFilesFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        _sut = new ExternalMergeSorter(new ExternalMergeSorterOptions { FileLocation = _fixture.FilesDirectory });
    }

    [Theory(Skip = "Only run manually")]
    [InlineData(10000000)]
    [InlineData(100000000)]
    public async Task FileSortedWithExternalMergeSortCommandShouldBeIdenticalToFileSortedWithArraySort(int rows)
    {
        var sourceFilename = _fixture.UnsortedFiles[rows];
        var sourceFullPath = Path.Combine(_fixture.FilesDirectory, sourceFilename);
        var sourceStream = File.OpenRead(sourceFullPath);
        var unsortedFileSize = sourceStream.Length;
        var targetFilename = $"{rows}.done";
        var targetFullPath = Path.Combine(_fixture.FilesDirectory, targetFilename);
        var target = File.OpenWrite(targetFullPath);

        await _sut.Sort(sourceStream, target, CancellationToken.None);
        var unsortedFileRows = await File.ReadAllLinesAsync(sourceFullPath);
        Array.Sort(unsortedFileRows);
        var arraySortedFilePath = Path.Combine(_fixture.FilesDirectory, "inmemory-sorted");
        await File.WriteAllLinesAsync(arraySortedFilePath, unsortedFileRows);
        await using var mergeSortedFile = File.OpenRead(targetFullPath);
        await using var arraySortedFile = File.OpenRead(arraySortedFilePath);

        mergeSortedFile.Length.ShouldBe(unsortedFileSize);
        var filesAreEqual = FileComparer.FilesAreEqual(mergeSortedFile, arraySortedFile);
        filesAreEqual.ShouldBeTrue();
        File.Delete(arraySortedFilePath);
        File.Delete(targetFullPath);
    }
}
