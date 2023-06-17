using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace JOS.Files.Implementations;

public class UploadFileBonusCommandAzure : IUploadFileCommand
{
    private readonly HttpClient _httpClient;

    public UploadFileBonusCommandAzure(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(UploadFileLocalCommand_Bytes));
        _httpClient.BaseAddress = new Uri("http://localhost:5000");
    }

    public async Task<HttpStatusCode> UploadFile(string filename)
    {
        var containerName = "fileupload";
        var blobClient = new BlobClient(connectionString: Config.AzureStorageConnectionString, blobContainerName: containerName, blobName: filename);
        var blob = await blobClient.DownloadAsync();
        var content = new StreamContent(blob.Value.Content);

        using (var request = new HttpRequestMessage(HttpMethod.Post, $"/files/{filename}.stream.azure")
               {
                   Content = content
               })
        {
            using (var response = await _httpClient.SendAsync(request))
            {
                return response.StatusCode;
            }
        }
    }
}