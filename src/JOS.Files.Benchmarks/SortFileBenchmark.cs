using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using JOS.Files.Implementations.Sorting;

namespace JOS.Files.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60)]
    [HtmlExporter]
    public class SortFileBenchmark
    {
        private readonly InMemorySortFileCommand _inMemorySortFileCommand;
        private readonly ExternalMergeSortFileCommand _externalMergeSortFileCommand;

        public SortFileBenchmark()
        {
            _inMemorySortFileCommand = new InMemorySortFileCommand();
            _externalMergeSortFileCommand = new ExternalMergeSortFileCommand();
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
        //    await _inMemorySortFileCommand.Execute(source, target);
        //}

        //[Benchmark]
        ////[Arguments(10)]
        ////[Arguments(100)]
        ////[Arguments(1000)]
        ////[Arguments(10000)]
        //[Arguments(100000)]
        ////[Arguments(1000000)]
        ////[Arguments(10000000)]
        //public async Task ExternalMergeSort(int rows)
        //{
        //    var source = File.OpenRead($"c:\\temp\\files\\unsorted.{rows}.csv");
        //    var target = File.OpenWrite($"c:\\temp\\files\\external-sorted.{rows}.csv");
        //    await _externalMergeSortFileCommand.Execute(source, target);
        //}

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
        //    await _inMemorySortFileCommand.Execute(source, target);
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
        //public async Task ExternalMergeSort_ServerGc(int rows)
        //{
        //    var source = File.OpenRead($"c:\\temp\\files\\unsorted.{rows}.csv");
        //    var target = File.OpenWrite($"c:\\temp\\files\\external-sorted.{rows}.csv");
        //    await _externalMergeSortFileCommand.Execute(source, target);
        //}

        //[Benchmark]
        //[Arguments(1)]
        //[Arguments(2)]
        //[Arguments(4)]
        //[Arguments(8)]
        //[Arguments(16)]
        //[Arguments(32)]
        //[Arguments(64)]
        //[Arguments(128)]
        //public async Task ChunkSize(int chunkSize)
        //{
        //    var command = new ExternalMergeSortFileCommand(new ExternalMergeSortOptions
        //    {
        //        Split = new ExternalMergeSortSplitOptions
        //        {
        //            ChunkSizeMb = chunkSize
        //        }
        //    });
        //    var source = File.OpenRead($"c:\\temp\\files\\unsorted.{1000000}.csv");
        //    var target = File.OpenWrite($"c:\\temp\\files\\external-sorted.{1000000}.csv");
        //    await command.Execute(source, target);
        //}

        [Benchmark]
        [Arguments(5)]
        [Arguments(10)]
        [Arguments(15)]
        public async Task Runs(int chunkSize)
        {
            var command = new ExternalMergeSortFileCommand(new ExternalMergeSortOptions
            {
                Split = new ExternalMergeSortSplitOptions
                {
                    ChunkSizeMb = 8
                },
                Merge = new ExternalMergeSortMergeOptions
                {
                    ChunkSize = chunkSize
                }
            });
            var source = File.OpenRead($"c:\\temp\\files\\unsorted.{1000000}.csv");
            var target = File.OpenWrite($"c:\\temp\\files\\external-sorted.{1000000}.csv");
            await command.Execute(source, target);
        }
    }
}
