using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace JOS.ExternalMergeSort.IntegrationTests;

public class FilesFixture : IAsyncLifetime
{
    public FilesFixture(int[] rows)
    {
        // ReSharper disable once PossibleNullReferenceException
        OutputDirectory =
            Directory.GetParent(typeof(ExternalMergeSort_SmallFiles_IntegrationTests).Assembly.Location)!.FullName;
        FilesDirectory = FileGenerator.FileLocation;
        Rows = rows ?? throw new ArgumentNullException(nameof(rows));
        UnsortedFiles = new Dictionary<int, string>();
    }

    protected string OutputDirectory { get; }
    public string FilesDirectory { get; }
    protected int[] Rows { get; }
    public Dictionary<int, string> UnsortedFiles { get; }

    public virtual async Task InitializeAsync()
    {
        if (!Directory.Exists(FilesDirectory))
        {
            Directory.CreateDirectory(FilesDirectory);
        }

        foreach (var row in Rows)
        {
            var filename = await FileGenerator.CreateFile(row, FilesDirectory);
            UnsortedFiles.Add(row, filename);
        }
    }

    public virtual Task DisposeAsync()
    {
        foreach (var unsortedFile in UnsortedFiles)
        {
            File.Delete(Path.Combine(FilesDirectory, unsortedFile.Value));
        }
        return Task.CompletedTask;
    }
}
