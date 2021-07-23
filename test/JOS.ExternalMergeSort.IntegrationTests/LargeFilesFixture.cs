using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JOS.Files.Implementations.Sorting;
using Xunit;

namespace JOS.ExternalMergeSort.IntegrationTests
{
    public class LargeFilesFixture : FilesFixture
    {
        public LargeFilesFixture() : base(new[]
        {
            10000000,
            100000000
        })
        {
        }

        private static bool RemoveUnsortedFilesWhenDone => false;

        public override async Task DisposeAsync()
        {
            if (RemoveUnsortedFilesWhenDone)
            {
                await base.DisposeAsync();
            }
        }
    }
}
