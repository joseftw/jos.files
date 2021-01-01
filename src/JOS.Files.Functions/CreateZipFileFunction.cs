using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JOS.Files.Functions
{
    public class FunctionCreateZipFileServiceBusTrigger
    {
        private const int ZipCompressionLevel = 1;
        private readonly ILogger<FunctionCreateZipFileServiceBusTrigger> _logger;
        private readonly string _storageConnectionString;

        public FunctionCreateZipFileServiceBusTrigger(
            IConfiguration configuration,
            ILogger<FunctionCreateZipFileServiceBusTrigger> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storageConnectionString = configuration.GetValue<string>("AzureWebJobsStorage") ?? throw new Exception("AzureWebJobsStorage was null");
        }

        [FunctionName("CreateZipFileServiceBus")]
        public async Task Run([ServiceBusTrigger(Queues.CreateZipFile, Connection = "ConnectionStrings:ServiceBus")]
            Message message,
            CancellationToken cancellationToken)
        {
            var messageData = JsonSerializer.Deserialize<GenerateZipFileMessage>(message.Body);
            var zipFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}.{Guid.NewGuid().ToString().Substring(0, 4)}.zip";
            var scopeParameters = new Dictionary<string, object>
            {
                { "ZipFileName", zipFileName },
                { "MessageId", message.MessageId },
            };
            using (_logger.BeginScope(scopeParameters))
            {
                _logger.LogInformation("Received message {MessageId}", message.MessageId);
                var zipBlobClient = new BlockBlobClient(_storageConnectionString, "zips", zipFileName);
                _logger.LogInformation("Starting to create {FileName}", zipFileName);
                var lastProgressLogged = DateTime.UtcNow;
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    var blockBlobOpenWriteOptions = CreateBlockBlobOpenWriteOptions(lastProgressLogged, stopwatch);
                    await using (var newZipFileStream = await zipBlobClient.OpenWriteAsync(true, blockBlobOpenWriteOptions, cancellationToken))
                    {
                        await using var zipStream = new ZipOutputStream(newZipFileStream, 5 * 1024 * 1024)
                        {
                            IsStreamOwner = false,
                            UseZip64 = UseZip64.On
                        };
                        zipStream.SetLevel(ZipCompressionLevel);
                        _logger.LogInformation("Using Level {ZipLevelCompression} compression", ZipCompressionLevel);
                        var entries = new List<ZipEntry>();
                        foreach (var filePath in messageData.FilePaths)
                        {
                            var blockBlobClient = CreateBlockBlobClient(messageData.ContainerName, filePath);
                            _logger.LogInformation("Starting work on {BlobName}", blockBlobClient.Name);
                            var blobName = blockBlobClient.Name;
                            var properties = await blockBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
                            var zipEntry = new ZipEntry(blobName)
                            {
                                Size = properties.Value.ContentLength
                            };
                            _logger.LogInformation("CRC BEFORE: {Crc}", zipEntry.Crc);
                            _logger.LogInformation("SIZE BEFORE: {Size}", zipEntry.Size);
                            zipStream.PutNextEntry(zipEntry);
                            entries.Add(zipEntry);
                            await blockBlobClient.DownloadToAsync(zipStream, transferOptions: new StorageTransferOptions
                            {
                                MaximumConcurrency = 10
                            }, cancellationToken: cancellationToken);
                            zipStream.CloseEntry();
                            _logger.LogInformation("CRC AFTER: {Crc}", zipEntry.Crc);
                            _logger.LogInformation("SIZE AFTER: {Size}", zipEntry.Size);
                        }
                        await newZipFileStream.FlushAsync(cancellationToken);
                        zipStream.Finish();
                    }
                    stopwatch.Stop();

                    _logger.LogInformation("Removed message {MessageId}", message.MessageId);
                    _logger.LogInformation("Zip file DONE, took {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error when creating zip file");
                    throw;
                }
            }
            
        }

        private BlockBlobClient CreateBlockBlobClient(string containerName, string filePath)
        {
            return new BlockBlobClient(_storageConnectionString, containerName, filePath);
        }

        private BlockBlobOpenWriteOptions CreateBlockBlobOpenWriteOptions(DateTime lastProgressLogged, Stopwatch stopwatch)
        {
            return new BlockBlobOpenWriteOptions
            {
                ProgressHandler = new Progress<long>(l =>
                {
                    if ((DateTime.UtcNow - lastProgressLogged).TotalSeconds >= 30)
                    {
                        lastProgressLogged = DateTime.UtcNow;
                        var megabytes = (l > 0 ? l : 1) / 1048576;
                        var seconds = stopwatch.ElapsedMilliseconds / 1000d;
                        var averageUploadMbps = Math.Round((l / seconds) / 125000d, 2);
                        _logger.LogInformation("Progress: {UploadedMB}MB Average Upload: {AverageUpload}Mbps", megabytes, averageUploadMbps);
                    }
                }),
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "application/zip"
                }
            };
        }
    }
}
