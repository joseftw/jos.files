using System.IO;

namespace JOS.ExternalMergeSort.IntegrationTests;

internal static class FileComparer
{
    internal static bool FilesAreEqual(Stream file1, Stream file2)
    {
        int file1byte;
        int file2byte;

        if (file1.Length != file2.Length)
        {
            file1.Close();
            file2.Close();

            return false;
        }

        do
        {
            // Read one byte from each file.
            file1byte = file1.ReadByte();
            file2byte = file2.ReadByte();
        }
        while ((file1byte == file2byte) && (file1byte != -1));

        // Close the files.
        file1.Close();
        file2.Close();

        return ((file1byte - file2byte) == 0);
    }
}