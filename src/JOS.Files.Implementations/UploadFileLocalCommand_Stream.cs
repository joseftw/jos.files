using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace JOS.Files.Implementations
{
    public class UploadFileLocalCommand_Stream : IUploadFileCommand
    {
        private readonly HttpClient _httpClient;

        public UploadFileLocalCommand_Stream(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(UploadFileLocalCommand_Bytes));
            _httpClient.BaseAddress = new Uri("http://localhost:5000");
        }

        public async Task<HttpStatusCode> UploadFile(string filename)
        {
            // DO THIS
            using (var file = File.OpenRead(Path.Combine(Config.ExampleFilesAbsolutePath, filename)))
            {
                var content = new StreamContent(file);
                var request = new HttpRequestMessage(HttpMethod.Post, $"/files/{filename}.stream")
                {
                    Content = content
                };
                using (var response = await _httpClient.SendAsync(request))
                {
                    return response.StatusCode;
                }
            }
        }
    }
}
