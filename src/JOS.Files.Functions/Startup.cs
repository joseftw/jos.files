using Azure.Messaging.ServiceBus;
using JOS.Files.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace JOS.Files.Functions

{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ServiceBusClient>(x =>
            {
                var configuration = x.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetValue<string>("ConnectionStrings:ServiceBus");
                return new ServiceBusClient(connectionString);
            });
            builder.Services.AddSingleton<ServiceBusSender>(x =>
            {
                var serviceBusClient = x.GetRequiredService<ServiceBusClient>();
                return serviceBusClient.CreateSender(Queues.CreateZipFile);
            });
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            base.ConfigureAppConfiguration(builder);
        }
    }
}
