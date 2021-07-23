using System.IO;
using JOS.Files.Implementations.Sorting;

namespace JOS.ExternalMergeSort.IntegrationTests
{
    public class FilesFixture
    {
        public FilesFixture()
        {
            // ReSharper disable once PossibleNullReferenceException
            OutputDirectory = Directory.GetParent(typeof(ExternalMergeSort_SmallFiles_IntegrationTests).Assembly.Location).FullName;
            FilesDirectory = FileGenerator.FileLocation;
        }

        protected string OutputDirectory { get; }
        public string FilesDirectory { get; protected set; }
    }
}
