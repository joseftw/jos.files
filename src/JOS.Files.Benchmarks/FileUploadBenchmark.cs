using System.Net;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using JOS.Files.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace JOS.Files.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net50, invocationCount: 2, warmupCount: 2, targetCount: 2, launchCount: 2)]
    [HtmlExporter]
    public class FileUploadBenchmark
    {
        private UploadFileLocalCommand_String _uploadFileLocalCommandString = null!;
        private UploadFileLocalCommand_Bytes _uploadFileLocalCommandBytes = null!;
        private UploadFileLocalCommand_Stream _uploadFileLocalCommandStream = null!;
        private UploadFileBonusCommandAzure _uploadFileBonusCommandAzure = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var servicesCollection = new ServiceCollection();
            var services = new DefaultServiceProviderFactory().CreateBuilder(servicesCollection);
            services.AddHttpClient();
            services.AddSingleton<UploadFileLocalCommand_String>();
            services.AddSingleton<UploadFileLocalCommand_Bytes>();
            services.AddSingleton<UploadFileLocalCommand_Stream>();
            services.AddSingleton<UploadFileBonusCommandAzure>();
            var serviceProvider = services.BuildServiceProvider();

            _uploadFileLocalCommandString = serviceProvider.GetRequiredService<UploadFileLocalCommand_String>();
            _uploadFileLocalCommandBytes = serviceProvider.GetRequiredService<UploadFileLocalCommand_Bytes>();
            _uploadFileLocalCommandStream = serviceProvider.GetRequiredService<UploadFileLocalCommand_Stream>();
            _uploadFileBonusCommandAzure = serviceProvider.GetRequiredService<UploadFileBonusCommandAzure>();
        }

        [Benchmark]
        [Arguments("1MB.test")]
        [Arguments("10MB.test")]
        [Arguments("100MB.test")]
        [Arguments("1000MB.test")]
        public async Task<HttpStatusCode> UploadFile_String(string filename)
        {
            var result = await _uploadFileLocalCommandString.UploadFile(filename);
            return result;
        }

        [Benchmark]
        [Arguments("1MB.test")]
        [Arguments("10MB.test")]
        [Arguments("100MB.test")]
        [Arguments("1000MB.test")]
        public async Task<HttpStatusCode> UploadFile_Bytes(string filename)
        {
            var result = await _uploadFileLocalCommandBytes.UploadFile(filename);
            return result;
        }

        [Benchmark]
        [Arguments("1MB.test")]
        [Arguments("10MB.test")]
        [Arguments("100MB.test")]
        [Arguments("1000MB.test")]
        public async Task<HttpStatusCode> UploadFile_Stream(string filename)
        {
            var result = await _uploadFileLocalCommandStream.UploadFile(filename);
            return result;
        }

        //[Benchmark]
        //[Arguments("1MB.test")]
        //[Arguments("10MB.test")]
        //[Arguments("100MB.test")]
        //[Arguments("1000MB.test")]
        //public async Task<HttpStatusCode> UploadFile_Azure(string filename)
        //{
        //    var result = await _uploadFileBonusCommandAzure.UploadFile(filename);
        //    return result;
        //}
    }
}
