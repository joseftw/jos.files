using System.Collections.Generic;

namespace JOS.Files.Functions
{
    public class GenerateZipFileMessage
    {
        public string ContainerName { get; set; }
        public IReadOnlyCollection<string> FilePaths { get; set; }
    }
}
