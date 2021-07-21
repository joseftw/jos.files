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
        private InMemorySortFileCommand _inMemorySortFileCommand;
        private ExternalMergeSortFileCommand _externalMergeSortFileCommand;

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            _inMemorySortFileCommand = new InMemorySortFileCommand();
            _externalMergeSortFileCommand = new ExternalMergeSortFileCommand();
            await Task.Delay(1);
            //await Task.WhenAll(
            //    FileGenerator.CreateFile(10),
            //    FileGenerator.CreateFile(100),
            //    FileGenerator.CreateFile(1000),
            //    FileGenerator.CreateFile(10000),
            //    FileGenerator.CreateFile(100000),
            //    FileGenerator.CreateFile(1000000),
            //    FileGenerator.CreateFile(10000000));
        }

        //[Benchmark(Baseline = true)]
        [Arguments(10)]
        [Arguments(100)]
        [Arguments(1000)]
        [Arguments(10000)]
        [Arguments(100000)]
        [Arguments(1000000)]
        [Arguments(10000000)]
        public async Task InMemory(int rows)
        {
            var source = File.OpenRead($"c:\\temp\\files\\unsorted.{rows}.csv");
            var target = File.OpenWrite($"c:\\temp\\files\\sorted.{rows}.csv");
            await _inMemorySortFileCommand.Execute(source, target);
        }

        [Benchmark]
        //[Arguments(10)]
        //[Arguments(100)]
        //[Arguments(1000)]
        //[Arguments(10000)]
        //[Arguments(100000)]
        //[Arguments(1000000)]
        [Arguments(10000000)]
        public async Task ExternalMergeSort(int rows)
        {
            var source = File.OpenRead($"c:\\temp\\files\\unsorted.{rows}.csv");
            var target = File.OpenWrite($"c:\\temp\\files\\sorted.{rows}.csv");
            await _externalMergeSortFileCommand.Execute(source, target);
        }
    }
}
