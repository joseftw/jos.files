# JOS.ExternalMergeSort

## Introduction
External Merge Sort implementation written in c# (dotnet).
More details can be found [here](https://josef.codes/sorting-really-large-files-with-c-sharp/)
## Usage

### Default
```csharp
var unsortedFile = File.OpenRead("MyUnsortedFile.csv");
var targetFile = File.OpenRead("MySortedFile.csv");
var sorter = new ExternalMergeSorter();

await sorter.Sort(unsortedFile, targetFile);
```

### Custom
```csharp
var unsortedFile = File.OpenRead("MyUnsortedFile.csv");
var targetFile = File.OpenRead("MySortedFile.csv");
var options = new ExternalMergeSorterOptions
{
    FileLocation = "/tmp"
};
var sorter = new ExternalMergeSorter(options);

await sorter.Sort(unsortedFile, targetFile);
```

It's possible to pass in custom ```IProgress``` handlers as well.
```csharp
var splitFileProgressHandler = new Progress<double>(x =>
{
    var percentage = x * 100;
    System.Console.WriteLine($"Split progress: {percentage:##.##}%");
});
var sortFilesProgressHandler = new Progress<double>(x =>
{
    var percentage = x * 100;
    System.Console.WriteLine($"Sort progress: {percentage:##.##}%");
});
var mergeFilesProgressHandler = new Progress<double>(x =>
{
    var percentage = x * 100;
    System.Console.WriteLine($"Merge progress: {percentage:##.##}%");
});

//var sortCommand = new ExternalMergeSorter(new ExternalMergeSorterOptions
{
    Split = new ExternalMergeSortSplitOptions
    {
        ProgressHandler = splitFileProgressHandler
    },
    Sort = new ExternalMergeSortSortOptions
    {
        ProgressHandler = sortFilesProgressHandler
    },
    Merge = new ExternalMergeSortMergeOptions
    {
        ProgressHandler = mergeFilesProgressHandler
    }
});

var sourceFile = Path.Combine(FileGenerator.FileLocation, sourceFilename);
targetFile = File.OpenWrite(Path.Combine(FileGenerator.FileLocation, $"sorted.{rows}.csv"));
await sortCommand.Sort(File.OpenRead(sourceFile), targetFile, CancellationToken.None);
```
