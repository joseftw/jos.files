using System.Net;
using System.Threading.Tasks;
using JOS.Files.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace JOS.Files.Implementation.Tests
{
    public class UploadFileLocalCommand1Tests
    {
        private readonly UploadFileLocalCommand_String _sut;

        public UploadFileLocalCommand1Tests()
        {
            var services = new ServiceCollection();
            services.AddHttpClient();
            services.AddTransient<UploadFileLocalCommand_String>();
            var serviceProvider = services.BuildServiceProvider();

            _sut = serviceProvider.GetRequiredService<UploadFileLocalCommand_String>();
        }

        [Theory]
        [InlineData("1MB.test")]
        [InlineData("10MB.test")]
        [InlineData("100MB.test")]
        [InlineData("1000MB.test")]
        public async Task UploadFile_ShouldReturn200OK(string filename)
        {
            var result = await _sut.UploadFile(filename);

            result.ShouldBe(HttpStatusCode.OK);
        }
    }
}
