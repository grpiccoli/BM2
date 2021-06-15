using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using BiblioMit.Services;

namespace BiblioMit
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateWebHostBuilder(args).Build().RunAsync().ConfigureAwait(false);
        }
        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
                webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureKestrel(options =>
                {
                    options.Limits.MaxConcurrentConnections = 200;
                    options.Limits.MaxConcurrentUpgradedConnections = 200;
                    //options.Limits.MaxRequestBodySize = 20_000_000;
                    //options.Limits.MinRequestBodyDataRate =
                    //    new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    //options.Limits.MinResponseDataRate =
                    //    new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                })
                .UseUrls("http://localhost:5018/")
                .UseStartup<Startup>())
            .ConfigureServices(services => 
            {
                services.AddHostedService<PlanktonBackground>();
            });
    }
}
