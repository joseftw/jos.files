﻿using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Bogus;
using CsvHelper;
using CsvHelper.Configuration;

namespace JOS.Files.Implementations.Sorting
{
    public static class FileGenerator
    {
        public static string FileLocation = "c:\\temp\\files";
        private static Faker<User> _userGenerator = null!;

        public static async Task<string> CreateFile(int rows, string location = "", bool overwrite = false)
        {
            _userGenerator = new Faker<User>()
                .RuleFor(u => u.Firstname, f => f.Name.FirstName())
                .RuleFor(u => u.Lastname, f => f.Name.LastName())
                .RuleFor(u => u.Avatar, f => f.Internet.Avatar())
                .RuleFor(u => u.Username, (f, u) => f.Internet.UserName(u.Firstname, u.Lastname))
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Firstname, u.Lastname))
                .RuleFor(u => u.SomethingUnique, f => $"Value {f.UniqueIndex}")
                .RuleFor(u => u.SomeGuid, Guid.NewGuid);

            var filename = $"unsorted.{rows}.csv";
            var path = string.IsNullOrWhiteSpace(location) ? Path.Combine(FileLocation, filename) : Path.Combine(location, filename);
            if (!overwrite)
            {
                if (File.Exists(path))
                {
                    Console.WriteLine($"File '{path}' already exists");
                    return filename;
                }
            }

            Console.WriteLine($"Creating {path}...");
            await using var writer = new StreamWriter(path, append: false);
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            for (int i = 0; i < rows; i++)
            {
                csv.WriteRecord(_userGenerator.Generate());
                await csv.NextRecordAsync();
            }

            await csv.DisposeAsync();
            await writer.DisposeAsync();

            return filename;
        }
    }

    public class User
    {
        public string Firstname { get; set; } = null!;
        public string Lastname { get; set; } = null!;
        public string FullName => $"{Firstname} {Lastname}";
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string SomethingUnique { get; set; } = null!;
        public Guid SomeGuid { get; set; }
        public string Avatar { get; set; } = null!;
    }

    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Map(x => x.Firstname).Index(1).Name("Firstname");
            Map(x => x.Lastname).Index(0).Name("Lastname");
            Map(x => x.FullName).Index(0).Name("Fullname");
            Map(x => x.Avatar).Index(0).Name("Avatar");
            Map(x => x.Username).Index(0).Name("Username");
            Map(x => x.Email).Index(0).Name("Email");
            Map(x => x.SomethingUnique).Index(0).Name("SomethingUnique");
            Map(x => x.SomeGuid).Index(0).Name("SomeGuid");
        }
    }
}
