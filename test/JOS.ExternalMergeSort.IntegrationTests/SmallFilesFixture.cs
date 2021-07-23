using System.Threading.Tasks;

namespace JOS.ExternalMergeSort.IntegrationTests
{
    public class SmallFilesFixture : FilesFixture
    {
        public SmallFilesFixture() : base(new[]
        {
            100,
            1000,
            10000,
            100000,
            1000000
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
