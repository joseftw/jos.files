﻿using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using JOS.Files.Implementations.Sorting;

namespace JOS.Files.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net80)]
    [HtmlExporter]
    public class SortFileBenchmark
    {
        private readonly InMemorySortCommand _inMemorySortCommand;
        private readonly ExternalMergeSortCommand _externalMergeSortCommand;

        public SortFileBenchmark()
        {
            _inMemorySortCommand = new InMemorySortCommand();
            _externalMergeSortCommand = new ExternalMergeSortCommand();
        }

        //[Benchmark(Baseline = true)]
        ////[Arguments(10)]
        ////[Arguments(100)]
        ////[Arguments(1000)]
        ////[Arguments(10000)]
        //[Arguments(100000)]
        ////[Arguments(1000000)]
        ////[Arguments(10000000)]
        //public async Task InMemory(int rows)
        //{
        //    var source = File.OpenRead($"c:\\temp\\files\\unsorted.{rows}.csv");
        //    var target = File.OpenWrite($"c:\\temp\\files\\inmemory-sorted.{rows}.csv");
        //    await _inMemorySortCommand.Execute(source, target);
        //}

        [Benchmark]
        [Arguments(10)]
        [Arguments(100)]
        [Arguments(1000)]
        [Arguments(10000)]
        [Arguments(100000)]
        [Arguments(1000000)]
        [Arguments(10000000)]
        public async Task ExternalMergeSorter_Recursive(int rows)
        {
            var source = File.OpenRead($"c:\\temp\\files\\unsorted.{rows}.csv");
            var target = File.OpenWrite($"c:\\temp\\files\\external-sorted.{rows}.csv");
            await _externalMergeSortCommand.Execute(source, target);
        }

        //[Benchmark(Baseline = true)]
        //[Arguments(10)]
        //[Arguments(100)]
        //[Arguments(1000)]
        //[Arguments(10000)]
        //[Arguments(100000)]
        //[Arguments(1000000)]
        //[Arguments(10000000)]
        //[GcServer(true)]
        //public async Task InMemory_ServerGc(int rows)
        //{
        //    var source = File.OpenRead($"c:\\temp\\files\\unsorted.{rows}.csv");
        //    var target = File.OpenWrite($"c:\\temp\\files\\inmemory-sorted.{rows}.csv");
        //    await _inMemorySortCommand.Execute(source, target);
        //}

        //[Benchmark]
        //[Arguments(10)]
        //[Arguments(100)]
        //[Arguments(1000)]
        //[Arguments(10000)]
        //[Arguments(100000)]
        //[Arguments(1000000)]
        //[Arguments(10000000)]
        //[GcServer(true)]
        //public async Task ExternalMergeSorter_ServerGc(int rows)
        //{
        //    var source = File.OpenRead($"c:\\temp\\files\\unsorted.{rows}.csv");
        //    var target = File.OpenWrite($"c:\\temp\\files\\external-sorted.{rows}.csv");
        //    await _externalMergeSortCommand.Execute(source, target);
        //}

        //[Benchmark]
        //[Arguments(1 * 1024 * 1024)]
        //[Arguments(2 * 1024 * 1024)]
        //[Arguments(4 * 1024 * 1024)]
        //[Arguments(8 * 1024 * 1024)]
        //[Arguments(16 * 1024 * 1024)]
        //[Arguments(32 * 1024 * 1024)]
        //[Arguments(64 * 1024 * 1024)]
        //[Arguments(128 * 1024 * 1024)]
        //public async Task ChunkSize(int chunkSizeBytes)
        //{
        //    var command = new ExternalMergeSorter(new ExternalMergeSorterOptions
        //    {
        //        Split = new ExternalMergeSortSplitOptions
        //        {
        //            FileSize = chunkSizeBytes
        //        }
        //    });
        //    var source = File.OpenRead($"c:\\temp\\files\\unsorted.{1000000}.csv");
        //    var target = File.OpenWrite($"c:\\temp\\files\\external-sorted.{1000000}.{Guid.NewGuid()}.benchmark");
        //    await command.Sort(source, target, CancellationToken.None);
        //}

        //[Benchmark]
        //[Arguments(5)]
        //[Arguments(10)]
        //[Arguments(15)]
        //public async Task FilesPerRun(int chunkSizeBytes)
        //{
        //    var command = new ExternalMergeSortFileCommand(new ExternalMergeSorterOptions
        //    {
        //        Split = new ExternalMergeSortSplitOptions
        //        {
        //            ChunkSize = 8 * 1024 * 1024
        //        },
        //        Merge = new ExternalMergeSortMergeOptions
        //        {
        //            ChunkSize = chunkSizeBytes
        //        }
        //    });
        //    var source = File.OpenRead($"c:\\temp\\files\\unsorted.{1000000}.csv");
        //    var target = File.OpenWrite($"c:\\temp\\files\\external-sorted.{1000000}.csv");
        //    await command.Execute(source, target);
        //}
    }
}
