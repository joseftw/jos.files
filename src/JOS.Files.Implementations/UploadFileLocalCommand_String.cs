using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JOS.Files.Implementations;

public class UploadFileLocalCommand_String : IUploadFileCommand
{
    private readonly HttpClient _httpClient;

    public UploadFileLocalCommand_String(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(UploadFileLocalCommand_String));
        _httpClient.BaseAddress = new Uri("http://localhost:5000");
    }

    public async Task<HttpStatusCode> UploadFile(string filename)
    {
        // DON'T DO THIS
        var file = File.ReadAllText(Path.Combine(Config.ExampleFilesAbsolutePath, filename));
        var content = new StringContent(file, Encoding.UTF8);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/files/{filename}.string")
        {
            Content = content
        };

        using var response = await _httpClient.SendAsync(request);
        return response.StatusCode;
    }
}