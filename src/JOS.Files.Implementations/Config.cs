using System;

namespace JOS.Files.Implementations
{
    public static class Config
    {
        public static string ExampleFilesAbsolutePath = "c:\\temp\\examplefiles";
        public static string DownloadFilesAbsolutePath = "c:\\temp\\download";
        public static string AzureStorageConnectionString = Environment.GetEnvironmentVariable("AzureStorageConnectionString") ?? throw new Exception("Missing AzureStorageConnectionString env variable");
    }
}
