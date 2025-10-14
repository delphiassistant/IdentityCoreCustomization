using IdentityCoreCustomization.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using IdentityCoreCustomization.Models.Identity;

// Use alias to avoid naming conflicts
using MsAuthenticationTicket = Microsoft.AspNetCore.Authentication.AuthenticationTicket;
using DbAuthenticationTicket = IdentityCoreCustomization.Models.Identity.AuthenticationTicket;

namespace IdentityCoreCustomization.Classes.Identity
{
    public class DatabaseTicketStore : ITicketStore
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DatabaseTicketStore(IHttpContextAccessor httpContextAccessor)
        {
           _httpContextAccessor = httpContextAccessor;
        }

        public async Task RemoveAsync(string key)
        {
            var db = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();
            if (int.TryParse(key, out var id))
            {
                var ticket = await db.AuthenticationTickets.FirstOrDefaultAsync(x => x.UserId == id);
                if (ticket != null)
                {
                    db.AuthenticationTickets.Remove(ticket);
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task RenewAsync(string key, MsAuthenticationTicket ticket)
        {
            var db = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();

            if (int.TryParse(key, out var id))
            {
                var authenticationTicket = await db.AuthenticationTickets.FirstOrDefaultAsync(at => at.UserId == id);
                if (authenticationTicket != null)
                {
                    authenticationTicket.Value = SerializeToBytes(ticket);
                    authenticationTicket.LastActivity = DateTimeOffset.UtcNow;
                    authenticationTicket.Expires = ticket.Properties.ExpiresUtc;
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<MsAuthenticationTicket> RetrieveAsync(string key)
        {
            var db = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();

            if (int.TryParse(key, out var id))
            {
                var authenticationTicket = await db.AuthenticationTickets.FirstOrDefaultAsync(at => at.UserId == id);
                if (authenticationTicket != null)
                {
                    authenticationTicket.LastActivity = DateTimeOffset.UtcNow;
                    await db.SaveChangesAsync();

                    return DeserializeFromBytes(authenticationTicket.Value);
                }
            }
            return null;
        }

        public async Task<string> StoreAsync(MsAuthenticationTicket ticket)
        {
            var db = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();

            var userId = string.Empty;
            var nameIdentifier = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (ticket.AuthenticationScheme == "Identity.Application")
            {
                userId = nameIdentifier;
            }
            // If using a external login provider like google we need to resolve the userid through the Userlogins
            else if (ticket.AuthenticationScheme == "Identity.External")
            {
                userId = (await db.UserLogins.SingleAsync(x => x.ProviderKey == nameIdentifier)).UserId.ToString();
            }
            
            var authenticationTicket = new DbAuthenticationTicket()
            {
                UserId = Convert.ToInt32(userId),
                LastActivity = DateTimeOffset.UtcNow,
                Value = SerializeToBytes(ticket),
            };

            var expiresUtc = ticket.Properties.ExpiresUtc;
            if (expiresUtc.HasValue)
            {
                authenticationTicket.Expires = expiresUtc.Value;
            }

            // Remove any existing tickets for this user (enforce single session per user)
            var existingTickets = await db.AuthenticationTickets.Where(at => at.UserId == Convert.ToInt32(userId)).ToListAsync();
            if (existingTickets.Any())
            {
                db.AuthenticationTickets.RemoveRange(existingTickets);
            }

            await db.AuthenticationTickets.AddAsync(authenticationTicket);
            await db.SaveChangesAsync();

            return authenticationTicket.UserId.ToString();
        }

        /// <summary>
        /// Gets all currently online users from database
        /// </summary>
        public async Task<List<OnlineUserSession>> GetAllOnlineUsersAsync()
        {
            var db = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();
            var currentTime = DateTimeOffset.UtcNow;

            var onlineUsers = await db.AuthenticationTickets
                .Include(at => at.User)
                .Where(at => !at.Expires.HasValue || at.Expires > currentTime)
                .Where(at => at.LastActivity.HasValue && at.LastActivity > currentTime.AddMinutes(-30)) // Consider active if activity within 30 minutes
                .Select(at => new OnlineUserSession
                {
                    UserId = at.UserId.ToString(),
                    UserName = at.User.UserName,
                    Email = at.User.Email,
                    PhoneNumber = at.User.PhoneNumber,
                    LoginTime = at.LastActivity.HasValue ? at.LastActivity.Value.AddHours(-1) : currentTime, // Approximate login time
                    LastActivity = at.LastActivity ?? currentTime,
                    ExpiresUtc = at.Expires,
                    TicketId = at.TicketID,
                    IsExpired = at.Expires.HasValue && at.Expires <= currentTime
                })
                .OrderByDescending(u => u.LastActivity)
                .ToListAsync();

            return onlineUsers;
        }

        /// <summary>
        /// Gets online sessions for a specific user
        /// </summary>
        public async Task<List<OnlineUserSession>> GetUserSessionsAsync(int userId)
        {
            var db = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();
            var currentTime = DateTimeOffset.UtcNow;

            var userSessions = await db.AuthenticationTickets
                .Include(at => at.User)
                .Where(at => at.UserId == userId)
                .Where(at => !at.Expires.HasValue || at.Expires > currentTime)
                .Select(at => new OnlineUserSession
                {
                    UserId = at.UserId.ToString(),
                    UserName = at.User.UserName,
                    Email = at.User.Email,
                    PhoneNumber = at.User.PhoneNumber,
                    LoginTime = at.LastActivity.HasValue ? at.LastActivity.Value.AddHours(-1) : currentTime,
                    LastActivity = at.LastActivity ?? currentTime,
                    ExpiresUtc = at.Expires,
                    TicketId = at.TicketID,
                    IsExpired = at.Expires.HasValue && at.Expires <= currentTime
                })
                .OrderByDescending(u => u.LastActivity)
                .ToListAsync();

            return userSessions;
        }

        /// <summary>
        /// Forces logout for a specific user by removing their authentication ticket
        /// </summary>
        public async Task<bool> ForceLogoutUserAsync(int userId)
        {
            var db = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();
            
            try
            {
                var userTickets = await db.AuthenticationTickets.Where(at => at.UserId == userId).ToListAsync();
                if (userTickets.Any())
                {
                    db.AuthenticationTickets.RemoveRange(userTickets);
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Forces logout for a specific session by ticket ID
        /// </summary>
        public async Task<bool> ForceLogoutSessionAsync(int ticketId)
        {
            var db = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();
            
            try
            {
                var ticket = await db.AuthenticationTickets.FirstOrDefaultAsync(at => at.TicketID == ticketId);
                if (ticket != null)
                {
                    db.AuthenticationTickets.Remove(ticket);
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Clears all authentication tickets (logs out all users)
        /// </summary>
        public async Task<int> ClearAllSessionsAsync()
        {
            var db = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();
            
            try
            {
                var allTickets = await db.AuthenticationTickets.ToListAsync();
                var count = allTickets.Count;
                
                if (allTickets.Any())
                {
                    db.AuthenticationTickets.RemoveRange(allTickets);
                    await db.SaveChangesAsync();
                }
                
                return count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Cleans up expired authentication tickets
        /// </summary>
        public async Task<int> CleanupExpiredSessionsAsync()
        {
            var db = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();
            var currentTime = DateTimeOffset.UtcNow;
            
            try
            {
                var expiredTickets = await db.AuthenticationTickets
                    .Where(at => at.Expires.HasValue && at.Expires <= currentTime)
                    .ToListAsync();
                
                var count = expiredTickets.Count;
                
                if (expiredTickets.Any())
                {
                    db.AuthenticationTickets.RemoveRange(expiredTickets);
                    await db.SaveChangesAsync();
                }
                
                return count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets count of currently active sessions
        /// </summary>
        public async Task<int> GetActiveSessionCountAsync()
        {
            var db = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();
            var currentTime = DateTimeOffset.UtcNow;
            
            return await db.AuthenticationTickets
                .Where(at => !at.Expires.HasValue || at.Expires > currentTime)
                .Where(at => at.LastActivity.HasValue && at.LastActivity > currentTime.AddMinutes(-30))
                .CountAsync();
        }

        /// <summary>
        /// Checks if a user is currently online
        /// </summary>
        public async Task<bool> IsUserOnlineAsync(int userId)
        {
            var db = _httpContextAccessor.HttpContext.RequestServices.GetService<ApplicationDbContext>();
            var currentTime = DateTimeOffset.UtcNow;
            
            return await db.AuthenticationTickets
                .Where(at => at.UserId == userId)
                .Where(at => !at.Expires.HasValue || at.Expires > currentTime)
                .Where(at => at.LastActivity.HasValue && at.LastActivity > currentTime.AddMinutes(-30))
                .AnyAsync();
        }

        private byte[] SerializeToBytes(MsAuthenticationTicket source)
            => TicketSerializer.Default.Serialize(source);

        private MsAuthenticationTicket DeserializeFromBytes(byte[] source)
            => source == null ? null : TicketSerializer.Default.Deserialize(source);
    }

    /// <summary>
    /// Represents an online user session from database
    /// </summary>
    public class OnlineUserSession
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTimeOffset LoginTime { get; set; }
        public DateTimeOffset LastActivity { get; set; }
        public DateTimeOffset? ExpiresUtc { get; set; }
        public int TicketId { get; set; }
        public bool IsExpired { get; set; }
        public TimeSpan SessionDuration => DateTimeOffset.UtcNow - LoginTime;
        public TimeSpan TimeSinceLastActivity => DateTimeOffset.UtcNow - LastActivity;
        public bool IsActive => TimeSinceLastActivity.TotalMinutes <= 30;
    }
}
