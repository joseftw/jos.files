using System;
using System.IO;
using System.Threading.Tasks;

namespace JOS.Files.Implementations.Sorting
{
    public static class FileGenerator
    {
        public static async Task CreateFile(int rows)
        {
            var random = new Random();
            var filename = $"unsorted.{rows}.txt";
            const int rowLength = 12;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ0123456789";
            await using var file = File.OpenWrite(filename);
            await using var streamWriter = new StreamWriter(file);
            for (var i = 0; i < rows; i++)
            {
                var @char = random.Next(chars.Length);
                await streamWriter.WriteLineAsync(new string(chars[@char], rowLength));
            }
        }
    }
}
