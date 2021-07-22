using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JOS.Files.Implementations.Sorting;
using Xunit;

namespace JOS.SortFile.IntegrationTests
{
    public class LargeFilesFixture : FilesFixture, IAsyncLifetime
    {
        public LargeFilesFixture()
        {
            Rows = new[]
            {
                10000000,
                100000000
            };
            Files = new Dictionary<int, string>();
        }

        public int[] Rows { get; }
        public Dictionary<int, string> Files { get; }
        private static bool RemoveUnsortedFilesWhenDone => false;


        public async Task InitializeAsync()
        {
            if (!Directory.Exists(FilesDirectory))
            {
                Directory.CreateDirectory(FilesDirectory);
            }

            foreach (var row in Rows)
            {
                var filename = await FileGenerator.CreateFile(row, FilesDirectory);
                Files.Add(row, filename);
            }
        }

        public Task DisposeAsync()
        {
            if (RemoveUnsortedFilesWhenDone)
            {
                foreach (var file in Files)
                {
                    File.Delete(Path.Combine(FilesDirectory, file.Value));
                }
            }

            return Task.CompletedTask;
        }
    }
}
