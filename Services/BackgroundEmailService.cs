using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityCoreCustomization.Services
{
    public class BackgroundEmailService : BackgroundService
    {
        private readonly IBackgroundEmailQueue _emailQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<BackgroundEmailService> _logger;

        public BackgroundEmailService(
            IBackgroundEmailQueue emailQueue,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<BackgroundEmailService> logger)
        {
            _emailQueue = emailQueue;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Email Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var (email, subject, htmlMessage) = await _emailQueue.DequeueAsync(stoppingToken);

                    _logger.LogInformation("Sending email to {Email} with subject '{Subject}'", email, subject);

                    // Create a new scope for each email send operation
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                        await emailSender.SendEmailAsync(email, subject, htmlMessage);
                    }

                    _logger.LogInformation("Email sent successfully to {Email}", email);
                }
                catch (OperationCanceledException)
                {
                    // Service is stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending email");
                    // Continue processing other messages
                }
            }

            _logger.LogInformation("Background Email Service stopped.");
        }
    }
}
