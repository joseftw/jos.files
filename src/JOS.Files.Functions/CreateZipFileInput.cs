using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JOS.Files.Functions
{
    public class CreateZipFileInput
    {
        [Required]
        public string ContainerName { get; set; }
        [Required(AllowEmptyStrings = false)]
        public IReadOnlyCollection<string> FilePaths { get; set; }
    }
}
