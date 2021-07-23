# JOS.ExternalMergeSort

## Introduction

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