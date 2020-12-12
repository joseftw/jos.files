using BenchmarkDotNet.Running;

namespace JOS.Files.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary1 = BenchmarkRunner.Run<FileUploadBenchmark>();
        }
    }
}
