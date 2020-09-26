using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BiblioMit.Services
{
    public class PlanktonBackground : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private Timer _timer;
        public PlanktonBackground(
            IServiceProvider services,
            IStringLocalizer<SeedBackground> localizer,
            ILogger<SeedBackground> logger)
        {
            _localizer = localizer;
            Services = services;
            _logger = logger;
        }
        public IServiceProvider Services { get; }
        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                _localizer["Plankton Service running is working."]);

            //_timer = new Timer(DoWork, null, TimeSpan.Zero, DateTime.Now);

            return Task.CompletedTask;
        }
        private void DoWork(object state)
        {
        }
        // noop
        public Task StopAsync(CancellationToken cancellationToken) 
        {
            _logger.LogInformation(
                _localizer["Plankton Service stopped."]);

            return Task.CompletedTask; 
        }
        public void Dispose()
        {
            _timer?.Dispose();
        }
        private static DateTime GetNextMidnight()
        {
            const string datePattern = "dd/MM/yyyy hh:mm:ss";
            const string dateFormat = "{0:00}/{1:00}/{2:0000} {3:00}:{4:00}:{5:00}";

            string dateString = string.Format(CultureInfo.CurrentCulture, dateFormat, DateTime.Now.Day + 1, DateTime.Now.Month, DateTime.Now.Year, 0, 0, 0);
            bool valid = DateTime.TryParseExact(dateString, datePattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime nextMidnight);

            if (!valid)
            {
                dateString = string.Format(CultureInfo.CurrentCulture, dateFormat, 1, DateTime.Now.Month + 1, DateTime.Now.Year, 0, 0, 0);
                valid = DateTime.TryParseExact(dateString, datePattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out nextMidnight);
            }

            if (!valid)
            {
                dateString = string.Format(CultureInfo.CurrentCulture, dateFormat, 1, 1, DateTime.Now.Year + 1, 0, 0, 0);
                DateTime.TryParseExact(dateString, datePattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out nextMidnight);
            }

            return nextMidnight;
        }
        private static DateTime LocalizeTime(DateTime input)
        {
            return TimeZoneInfo.ConvertTime(input, TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"));
        }
    }
}
