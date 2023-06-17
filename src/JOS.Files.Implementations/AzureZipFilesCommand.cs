using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using ICSharpCode.SharpZipLib.Zip;

namespace JOS.Files.Implementations;

public class AzureZipFilesCommand : IZipFilesCommand
{
    public async Task Execute(IReadOnlyCollection<string> filenames)
    {
        var containerName = "fileupload";

        using (var targetStream = File.Create(Path.Combine(Config.DownloadFilesAbsolutePath, "files.zip")))
        {
            using (var zipOutputStream = new ZipOutputStream(targetStream))
            {
                foreach (var filename in filenames)
                {
                    var entry = new ZipEntry(filename)
                    {
                        DateTime = DateTime.UtcNow,
                    };
                    zipOutputStream.PutNextEntry(entry);
                    var blobClient = new BlobClient(connectionString: Config.AzureStorageConnectionString, blobContainerName: containerName, blobName: filename);
                    await blobClient.DownloadToAsync(zipOutputStream);
                    await targetStream.FlushAsync();
                }
                zipOutputStream.Finish();
                zipOutputStream.Close();
            }
        }
    }
}