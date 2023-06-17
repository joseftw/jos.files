using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace JOS.Files.Functions;

public class CreateZipHttpFunction
{
    [FunctionName("CreateZipHttpFunction")]
    public ActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "zip")] CreateZipFileInput input,
        [Queue(Queues.CreateZipFile, Connection = "FilesStorageConnectionString")] ICollector<CreateZipFileMessage> queueCollector,
        HttpRequest request)
    {
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), validationResults))
        {
            return new BadRequestObjectResult(new
            {
                errors = validationResults.Select(x => x.ErrorMessage)
            });
        }
            
        queueCollector.Add(new CreateZipFileMessage
        {
            ContainerName = input.ContainerName,
            FilePaths = input.FilePaths
        });
            
        return new AcceptedResult();
    }
}