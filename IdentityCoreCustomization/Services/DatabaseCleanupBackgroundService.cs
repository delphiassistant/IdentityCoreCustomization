using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityCoreCustomization.Services
{
    public class DatabaseCleanupBackgroundService : BackgroundService
    {
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromSeconds(20);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DatabaseCleanupBackgroundService> _logger;

        public DatabaseCleanupBackgroundService(IServiceScopeFactory scopeFactory, ILogger<DatabaseCleanupBackgroundService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Database cleanup background service started. Interval: {Interval}", CleanupInterval);

            // Run immediately on startup
            await RunCleanupAsync(stoppingToken);

            using var timer = new PeriodicTimer(CleanupInterval);
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await RunCleanupAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Expected during shutdown
            }

            _logger.LogInformation("Database cleanup background service is stopping.");
        }

        private async Task RunCleanupAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var cleaner = scope.ServiceProvider.GetRequiredService<IDatabaseCleanerService>();

            try
            {
                await cleaner.CleanDatabaseAsync();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Propagate cancellation when requested
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running database cleanup job");
            }
        }
    }
}
