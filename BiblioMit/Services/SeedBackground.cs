using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BiblioMit.Services
{
    public class SeedBackground : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        //private Timer _timer;
        public SeedBackground(
            IServiceProvider services,
            IStringLocalizer<SeedBackground> localizer,
            ILogger<SeedBackground> logger)
        {
            _localizer = localizer;
            Services = services;
            _logger = logger;
        }
        public IServiceProvider Services { get; }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                _localizer["Consume Scoped Service Hosted Service is working."]);

            using var scope = Services.CreateScope();
            var scopedProcessingService = scope.ServiceProvider.GetRequiredService<ISeed>();

            try
            {
                await scopedProcessingService.Seed().ConfigureAwait(false);
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, "");
                throw;
            }
        }
        // noop
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
