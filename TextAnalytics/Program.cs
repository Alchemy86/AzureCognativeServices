using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace TextAnalytics
{
    class Program
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("<replace-with-your-text-analytics-key-here>");
        private static readonly Uri endpoint = new Uri("<replace-with-your-text-analytics-endpoint-here>");

        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.AddEnvironmentVariables("ASPNETCORE_");
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(Directory.GetCurrentDirectory());
                    configApp.AddJsonFile("appsettings.json", optional: true);
                    configApp.AddJsonFile(
                        $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true);
                        configApp.AddUserSecrets<Program>();
                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        configApp.AddUserSecrets<Program>();
                    }
                    configApp.AddCommandLine(args);
                })
                .ConfigureLogging((hostBuilder, lb) =>
                {
                    var configuration = new ConfigurationBuilder()
                        .AddJsonFile(@"Serilog.json", false);

                    if (hostBuilder.HostingEnvironment.IsDevelopment())
                        configuration.AddUserSecrets<Program>();

                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration.Build())
                        .CreateLogger();

                    lb.AddSerilog();
 
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.AddSingleton<IHostedService, HostedService>();
                    services.Configure<AzureSettings>(hostContext.Configuration.GetSection("AzureSettings"));
                })
                .UseConsoleLifetime()
                .Build();

            await hostBuilder.RunAsync();
        }
    }
}
