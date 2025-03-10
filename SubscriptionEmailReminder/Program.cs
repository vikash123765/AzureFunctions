using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Subscription5DaysExpiryNotification.Services;
using System;
using System.IO;

public class Program
{
    public static void Main()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        Console.WriteLine($"? Debug: HostEndpoint = {config["Functions:Worker:HostEndpoint"]}");

        var host = new HostBuilder()
            .ConfigureFunctionsWebApplication()  // ? Corrected here
            .ConfigureServices((hostBuilderContext, services) =>
            {
                services.AddSingleton<IConfiguration>(config);
                services.AddHttpClient();
                services.AddSingleton<EmailSender>();
                services.AddLogging();
            })
            .Build();

        host.Run();
    }
}
