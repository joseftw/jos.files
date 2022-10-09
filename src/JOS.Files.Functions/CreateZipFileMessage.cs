using System.Collections.Generic;

namespace JOS.Files.Functions
{
    public class CreateZipFileMessage
    {
        public string ContainerName { get; set; } = null!;
        public IReadOnlyCollection<string> FilePaths { get; set; } = new List<string>();
    }
}
