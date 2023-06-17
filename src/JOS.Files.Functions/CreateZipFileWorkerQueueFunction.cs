using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace JOS.Files.Functions;

public class CreateZipFileWorkerQueueFunction
{
    private readonly ICreateZipFileCommand _createZipFileCommand;
    private readonly ILogger<CreateZipFileWorkerQueueFunction> _logger;

    public CreateZipFileWorkerQueueFunction(
        ICreateZipFileCommand createZipFileCommand,
        ILogger<CreateZipFileWorkerQueueFunction> logger)
    {
        _createZipFileCommand = createZipFileCommand ?? throw new ArgumentNullException(nameof(createZipFileCommand));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
        
    [FunctionName("CreateZipFileWorkerQueueFunction")]
    public async Task Run([QueueTrigger(
            queueName: Queues.CreateZipFile,
            Connection = "FilesStorageConnectionString")]
        CreateZipFileMessage message,
        string id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting to process message {MessageId}", id);
        await _createZipFileCommand.Execute(message.ContainerName, message.FilePaths, cancellationToken);
        _logger.LogInformation("Processing of message {MessageId} done", id);
    }
}