using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace JOS.ExternalMergeSort.IntegrationTests
{
    [Collection("SmallFiles")]
    public class ExternalMergeSort_SmallFiles_IntegrationTests : IClassFixture<SmallFilesFixture>
    {
        private readonly SmallFilesFixture _fixture;
        private readonly JOS.ExternalMergeSort.ExternalMergeSort _sut;

        public ExternalMergeSort_SmallFiles_IntegrationTests(SmallFilesFixture fixture)
        {
            _fixture = fixture;
            _sut = new JOS.ExternalMergeSort.ExternalMergeSort(new ExternalMergeSortOptions{FileLocation = _fixture.FilesDirectory});
        }

        [Theory]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        [InlineData(1000000)]
        public async Task FileSortedWithExternalMergeSortCommandShouldBeIdenticalToFileSortedWithArraySort(int rows)
        {
            var sourceFilename = _fixture.Files[rows];
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
}
