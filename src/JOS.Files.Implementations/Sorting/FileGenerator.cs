using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using CsvHelper;
using CsvHelper.Configuration;

namespace JOS.Files.Implementations.Sorting
{
    public static class FileGenerator
    {
        public static string FileLocation = "c:\\temp\\files";
        private static readonly Faker<User> UserGenerator;

        static FileGenerator()
        {
            UserGenerator = new Faker<User>()
                .RuleFor(u => u.Firstname, f => f.Name.FirstName())
                .RuleFor(u => u.Lastname, f => f.Name.LastName())
                .RuleFor(u => u.Avatar, f => f.Internet.Avatar())
                .RuleFor(u => u.Username, (f, u) => f.Internet.UserName(u.Firstname, u.Lastname))
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Firstname, u.Lastname))
                .RuleFor(u => u.SomethingUnique, f => $"Value {f.UniqueIndex}")
                .RuleFor(u => u.SomeGuid, Guid.NewGuid);
        }

        public static async Task<string> CreateFile(int rows, string location = "")
        {
            var filename = $"unsorted.{rows}.csv";
            var path = string.IsNullOrWhiteSpace(location) ? filename : Path.Combine(location, filename);
            await using var writer = new StreamWriter(path, append: false, Encoding.UTF8, 100 * 1024 * 1024);
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            for (var i = 0; i < rows; i++)
            {
                var user = UserGenerator.Generate();
                csv.WriteRecord(user);
                await csv.NextRecordAsync();
            }

            return filename;
        }
    }

    public class User
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string FullName => $"{Firstname} {Lastname}";
        public string Username { get; set; }
        public string Email { get; set; }
        public string SomethingUnique { get; set; }
        public Guid SomeGuid { get; set; }
        public string Avatar { get; set; }
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
