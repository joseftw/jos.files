using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace JOS.Files.Functions
{
    public class TriggerZipFileCreation
    {
        private readonly ServiceBusSender _serviceBusSender;
        private readonly ILogger<TriggerZipFileCreation> _logger;

        public TriggerZipFileCreation(ServiceBusSender serviceBusSender, ILogger<TriggerZipFileCreation> logger)
        {
            _serviceBusSender = serviceBusSender ?? throw new ArgumentNullException(nameof(serviceBusSender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        [FunctionName("TriggerZipFileCreation")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "zip")]
            TriggerZipFileInput input)
        {
            _logger.LogInformation("Received TriggerZipFile Request");
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(input, new ValidationContext(input), validationResults))
            {
                return new BadRequestObjectResult(new
                {
                    errors = validationResults.Select(x => x.ErrorMessage)
                });
            }

            var messageData = new GenerateZipFileMessage
            {
                ContainerName = input.ContainerName,
                FilePaths = input.FilePaths
            };
            var message = new ServiceBusMessage(JsonSerializer.Serialize(messageData))
            {
                ContentType = "application/json"
            };

            _logger.LogInformation("Publishing GenerateZipFileMessage");
            await _serviceBusSender.SendMessageAsync(message);
            _logger.LogInformation("Published GenerateZipFileMessage");

            return new AcceptedResult();
        }
    }
}
