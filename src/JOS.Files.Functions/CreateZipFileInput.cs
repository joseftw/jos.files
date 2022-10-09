using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JOS.Files.Functions
{
    public class CreateZipFileInput
    {
        [Required] public string ContainerName { get; set; } = null!;

        [Required(AllowEmptyStrings = false)]
        public IReadOnlyCollection<string> FilePaths { get; set; } = new List<string>();
    }
}
