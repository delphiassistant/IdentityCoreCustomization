using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityCoreCustomization.Services
{
    public class BackgroundSmsService : BackgroundService
    {
        private readonly IBackgroundSmsQueue _smsQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<BackgroundSmsService> _logger;

        public BackgroundSmsService(
            IBackgroundSmsQueue smsQueue,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<BackgroundSmsService> logger)
        {
            _smsQueue = smsQueue;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background SMS Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var (smsText, phones) = await _smsQueue.DequeueAsync(stoppingToken);

                    _logger.LogInformation("Sending SMS to {Count} recipient(s)", phones.Count);

                    // Create a new scope for each SMS send operation
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var smsService = scope.ServiceProvider.GetRequiredService<ISmsService>();
                        await smsService.SendSms(smsText, phones);
                    }

                    _logger.LogInformation("SMS sent successfully to {Count} recipient(s)", phones.Count);
                }
                catch (OperationCanceledException)
                {
                    // Service is stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending SMS");
                    // Continue processing other messages
                }
            }

            _logger.LogInformation("Background SMS Service stopped.");
        }
    }
}
