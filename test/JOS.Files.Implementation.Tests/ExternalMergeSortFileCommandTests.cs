using System.IO;
using System.Threading.Tasks;
using JOS.Files.Implementations.Sorting;
using Xunit;

namespace JOS.Files.Implementation.Tests
{
    public class ExternalMergeSortFileCommandTests : IAsyncLifetime
    {
        private const int NumberOfRows = 1_000_000;
        private static string Filename;
        private readonly ExternalMergeSortFileCommand _sut;

        public ExternalMergeSortFileCommandTests()
        {
            _sut = new ExternalMergeSortFileCommand();
        }

        public async Task InitializeAsync()
        {
            // TODO REMOVE TEMP CODE
            Filename = $"unsorted.{NumberOfRows}.csv";
            if (!File.Exists(Filename))
            {
                Filename = await FileGenerator.CreateFile(NumberOfRows);
            }
        }

        public Task DisposeAsync()
        {
            // TODO REMOVE TEMP CODE
            //File.Delete(Filename);
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Hej()
        {
            var fileStream = File.OpenRead(Filename);

            await _sut.Execute(fileStream);
        }
    }
}
