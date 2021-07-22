using System.IO;
using JOS.Files.Implementations.Sorting;

namespace JOS.SortFile.IntegrationTests
{
    public class FilesFixture
    {
        public FilesFixture()
        {
            // ReSharper disable once PossibleNullReferenceException
            OutputDirectory = Directory.GetParent(typeof(ExternalMergeSortIntegrationTests).Assembly.Location).FullName;
            FilesDirectory = FileGenerator.FileLocation;
        }

        protected string OutputDirectory { get; }
        public string FilesDirectory { get; protected set; }
    }
}
