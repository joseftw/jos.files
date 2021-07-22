using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using JOS.Files.Implementations.Sorting;

namespace JOS.Files.Benchmarks
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await FileGenerator.CreateFile(10);
            await FileGenerator.CreateFile(100);
            await FileGenerator.CreateFile(1000);
            await FileGenerator.CreateFile(10000);
            await FileGenerator.CreateFile(100000);
            await FileGenerator.CreateFile(1000000);
            await FileGenerator.CreateFile(10000000);

            var summary = BenchmarkRunner.Run<SortFileBenchmark>();
        }
    }
}
