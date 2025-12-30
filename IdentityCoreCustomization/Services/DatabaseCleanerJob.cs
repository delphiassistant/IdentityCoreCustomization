using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IdentityCoreCustomization.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityCoreCustomization.Services
{
    public interface IDatabaseCleanerService
    {
        Task CleanDatabaseAsync();
    }
    
    public class DatabaseCleanerService : IDatabaseCleanerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseCleanerService> _logger;

        public DatabaseCleanerService(ApplicationDbContext context, ILogger<DatabaseCleanerService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task CleanDatabaseAsync()
        {
            try
            {
                var currentTime = DateTime.Now;
                var cleanupStartTime = DateTime.Now;
                
                _logger.LogInformation("Starting database cleanup job at {StartTime}", cleanupStartTime);

                // Remove expired SMS logins
                var expiredSmsLogins = await _context.UserLoginWithSms
                    .Where(l => l.ExpireDate < currentTime)
                    .ToListAsync();

                if (expiredSmsLogins.Any())
                {
                    _context.UserLoginWithSms.RemoveRange(expiredSmsLogins);
                    _logger.LogInformation("Removed {Count} expired SMS login attempts", expiredSmsLogins.Count);
                }

                // Remove expired pre-registrations (note: property name is PreRegistrations)
                var expiredPreRegistrations = await _context.UserPhoneTokens
                    .Where(l => l.ExpireTime < currentTime)
                    .ToListAsync();

                if (expiredPreRegistrations.Any())
                {
                    _context.UserPhoneTokens.RemoveRange(expiredPreRegistrations);
                    _logger.LogInformation("Removed {Count} expired pre-registrations", expiredPreRegistrations.Count);
                }

                // Remove expired authentication tickets (optional - you might want to keep these longer)
                var expiredTickets = await _context.AuthenticationTickets
                    .Where(t => t.Expires.HasValue && t.Expires < DateTimeOffset.UtcNow)
                    .ToListAsync();

                if (expiredTickets.Any())
                {
                    _context.AuthenticationTickets.RemoveRange(expiredTickets);
                    _logger.LogInformation("Removed {Count} expired authentication tickets", expiredTickets.Count);
                }

                // Save all changes
                var totalChanges = await _context.SaveChangesAsync();
                
                var cleanupEndTime = DateTime.Now;
                var duration = cleanupEndTime - cleanupStartTime;
                
                _logger.LogInformation("Database cleanup completed in {Duration}ms. Total records removed: {TotalChanges}", 
                    duration.TotalMilliseconds, totalChanges);
                
                // Keep the Debug.Print for backward compatibility if needed
                Debug.Print($"{DateTime.Now:HH:mm:ss} : DatabaseCleanerJob completed. Removed {totalChanges} records.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database cleanup");
            }
        }
    }
}
