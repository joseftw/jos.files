using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace JOS.FileApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FilesController : ControllerBase
    {
        [HttpPost("{filename}")]
        public async Task<IActionResult> Upload(string filename)
        {
            var filePath = Path.Combine("c:\\", "temp", filename);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await Request.Body.CopyToAsync(fileStream);
            }

            return new OkResult();
        }
    }
}
