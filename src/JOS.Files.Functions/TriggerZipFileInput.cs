using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JOS.Files.Functions
{
    public class TriggerZipFileInput
    {
        [Required]
        public string ContainerName { get; set; }
        [Required]
        public IReadOnlyCollection<string> FilePaths { get; set; }
    }
}
