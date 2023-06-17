using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JOS.Files.Functions;

public class AzureBlobStorageCreateZipFileCommand : ICreateZipFileCommand
{
    private readonly UploadProgressHandler _uploadProgressHandler;
    private readonly ILogger<AzureBlobStorageCreateZipFileCommand> _logger;
    private readonly string _storageConnectionString;
    private readonly string _zipStorageConnectionString;
        
    public AzureBlobStorageCreateZipFileCommand(
        IConfiguration configuration,
        UploadProgressHandler uploadProgressHandler,
        ILogger<AzureBlobStorageCreateZipFileCommand> logger)
    {
        _uploadProgressHandler = uploadProgressHandler ?? throw new ArgumentNullException(nameof(uploadProgressHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _storageConnectionString = configuration.GetValue<string>("FilesStorageConnectionString") ?? throw new Exception("FilesStorageConnectionString was null");
        _zipStorageConnectionString = configuration.GetValue<string>("ZipStorageConnectionString") ?? throw new Exception("ZipStorageConnectionString was null");
    }
        
    public async Task Execute(
        string containerName,
        IReadOnlyCollection<string> filePaths,
        CancellationToken cancellationToken)
    {
        var zipFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}.{Guid.NewGuid().ToString().Substring(0, 4)}.zip";
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using (var zipFileStream = await OpenZipFileStream(zipFileName, cancellationToken))
            {
                using (var zipFileOutputStream = CreateZipOutputStream(zipFileStream))
                {
                    var level = 0;
                    _logger.LogInformation("Using Level {Level} compression", level);
                    zipFileOutputStream.SetLevel(level);
                    foreach (var filePath in filePaths)
                    {
                        var blockBlobClient = new BlockBlobClient(_storageConnectionString, containerName, filePath);
                        var properties = await blockBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
                        var zipEntry = new ZipEntry(blockBlobClient.Name)
                        {
                            Size = properties.Value.ContentLength
                        };
                        zipFileOutputStream.PutNextEntry(zipEntry);
                        await blockBlobClient.DownloadToAsync(zipFileOutputStream, cancellationToken);
                        zipFileOutputStream.CloseEntry();
                    }
                }
            }

            stopwatch.Stop();
            _logger.LogInformation("[{ZipFileName}] DONE, took {ElapsedTime}", zipFileName, stopwatch.Elapsed);
        }
        catch (TaskCanceledException)
        {
            var blockBlobClient = new BlockBlobClient(_zipStorageConnectionString, "zips", zipFileName);
            await blockBlobClient.DeleteIfExistsAsync();
            throw;
        }
    }

    private async Task<Stream> OpenZipFileStream(
        string zipFilename,
        CancellationToken cancellationToken)
    {
        var zipBlobClient = new BlockBlobClient(_zipStorageConnectionString, "zips", zipFilename);
            
        return await zipBlobClient.OpenWriteAsync(true, options: new BlockBlobOpenWriteOptions
        {
            ProgressHandler = _uploadProgressHandler,
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = "application/zip"
            }
        }, cancellationToken: cancellationToken);
    }

    private static ZipOutputStream CreateZipOutputStream(Stream zipFileStream)
    {
        return new ZipOutputStream(zipFileStream)
        {
            IsStreamOwner = false
        };
    }
}