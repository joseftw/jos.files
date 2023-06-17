using System.Collections.Generic;
using System.Threading.Tasks;
using JOS.Files.Implementations;
using Xunit;

namespace JOS.Files.Implementation.Tests;

public class AzureDownloadAndZipFilesCommandTests
{
    private readonly AzureZipFilesCommand _sut;

    public AzureDownloadAndZipFilesCommandTests()
    {
        _sut = new AzureZipFilesCommand();
    }

    [Fact]
    public async Task ShouldCreateZipFileContainingAllFiles()
    {
        var files = new List<string>
        {
            "1MB.test",
            "10MB.test",
            "100MB.test",
            "1000MB.test",
        };

        await _sut.Execute(files);
    }
}