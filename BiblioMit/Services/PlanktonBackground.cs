using BiblioMit.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace BiblioMit.Services
{
    public class PlanktonBackground : IHostedService, IDisposable
    {
        private bool _disposed;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private Timer _timer;
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new();
        public PlanktonBackground(
            IServiceProvider services,
            IStringLocalizer<PlanktonBackground> localizer,
            ILogger<PlanktonBackground> logger)
        {
            _localizer = localizer;
            Services = services;
            _logger = logger;
        }
        public IServiceProvider Services { get; }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                _localizer["Plankton Service running is working."]);
            var time = TimeToNextSaturdayMidnight();
            //_timer = new Timer(FetchAssays, null, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(-1));
            _timer = new Timer(FetchAssays, null, TimeToNextSaturdayMidnight(), TimeSpan.FromMilliseconds(-1));

            return Task.CompletedTask;
        }
        private void FetchAssays(object state)
        {
            _timer?.Change(Timeout.Infinite, 0);
            _executingTask = FetchAssaysAsync(_stoppingCts.Token);
        }
        private async Task FetchAssaysAsync(CancellationToken stoppingToken)
        {
            using var scope = Services.CreateScope();
            var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IPlanktonService>();

            try
            {
                await scopedProcessingService.PullRecordsAsync(stoppingToken).ConfigureAwait(false);
                _timer.Change(TimeToNextSaturdayMidnight(), TimeSpan.FromMilliseconds(-1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        // noop
        public async Task StopAsync(CancellationToken cancellationToken) 
        {
            _logger.LogInformation(
                _localizer["Plankton Service stopped."]);
            _timer?.Change(Timeout.Infinite, 0);

            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _timer?.Dispose();
                _stoppingCts?.Dispose();
            }
            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
            _disposed = true;
        }
        private static TimeSpan TimeToNextSaturdayMidnight()
        {
            const string datePattern = "dd/MM/yyyy hh:mm:ss";
            const string dateFormat = "{0:00}/{1:00}/{2:0000} {3:00}:{4:00}:{5:00}";

            var now = DateTime.Now;

            int daysUntilNextSaturday = (DayOfWeek.Saturday - now.DayOfWeek + 7) % 7;

            now.AddDays(daysUntilNextSaturday);

            string dateString = string.Format(CultureInfo.CurrentCulture, dateFormat, now.Day + 1, now.Month, now.Year, 0, 0, 0);
            bool valid = DateTime.TryParseExact(dateString, datePattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime nextMidnight);

            if (!valid)
            {
                dateString = string.Format(CultureInfo.CurrentCulture, dateFormat, 1, now.Month + 1, now.Year, 0, 0, 0);
                valid = DateTime.TryParseExact(dateString, datePattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out nextMidnight);
            }

            if (!valid)
            {
                dateString = string.Format(CultureInfo.CurrentCulture, dateFormat, 1, 1, now.Year + 1, 0, 0, 0);
                DateTime.TryParseExact(dateString, datePattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out nextMidnight);
            }

            return nextMidnight.Subtract(DateTime.Now);
        }
        //private static DateTime LocalizeTime(DateTime input)
        //{
        //    return TimeZoneInfo.ConvertTime(input, TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"));
        //}
    }
}
