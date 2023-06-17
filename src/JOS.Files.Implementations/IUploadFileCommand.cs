using System.Net;
using System.Threading.Tasks;

namespace JOS.Files.Implementations;

public interface IUploadFileCommand
{
    Task<HttpStatusCode> UploadFile(string filename);
}