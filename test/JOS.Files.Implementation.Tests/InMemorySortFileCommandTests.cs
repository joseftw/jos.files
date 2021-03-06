﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JOS.Files.Implementations.Sorting;
using Shouldly;
using Xunit;

namespace JOS.Files.Implementation.Tests
{
    public class InMemorySortFileCommandTests : IAsyncLifetime
    {
        private readonly InMemorySortFileCommand _sut;

        public InMemorySortFileCommandTests()
        {
            _sut = new InMemorySortFileCommand();
        }

        [Fact]
        public async Task ShouldSortFileInAscendingOrder()
        {
            var unsortedFile = File.OpenRead("unsorted.txt");

            await _sut.Execute(unsortedFile);
            using var streamReader = new StreamReader(File.OpenRead("sorted.txt"));
            var lines = new List<string>();
            while (!streamReader.EndOfStream)
            {
                lines.Add(await streamReader.ReadLineAsync());
            }
            var sortedLines = lines.OrderBy(x => x).ToList();

            sortedLines.ShouldBeEquivalentTo(lines);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            File.Delete("sorted.txt");
            return Task.CompletedTask;
        }
    }
}
