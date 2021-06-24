using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using JOS.Files.Implementations.Sorting;

namespace JOS.Files.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [HtmlExporter]
    public class SortFileBenchmark
    {
        private InMemorySortFileCommand _inMemorySortFileCommand;

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            _inMemorySortFileCommand = new InMemorySortFileCommand();

            await FileGenerator.CreateFile(10);
            await FileGenerator.CreateFile(100);
            await FileGenerator.CreateFile(1000);
            await FileGenerator.CreateFile(10000);
            await FileGenerator.CreateFile(100000);
            await FileGenerator.CreateFile(1000000);
        }

        [Benchmark(Baseline = true)]
        [Arguments(10)]
        [Arguments(100)]
        [Arguments(1000)]
        [Arguments(10000)]
        [Arguments(100000)]
        [Arguments(1000000)]
        public async Task SortFile_InMemory(int rows)
        {
            var file = File.OpenRead($"unsorted.{rows}.txt");
            await _inMemorySortFileCommand.Execute(file);
        }
    }
}
