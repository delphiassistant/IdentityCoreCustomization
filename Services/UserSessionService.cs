using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityCoreCustomization.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace IdentityCoreCustomization.Services
{
    public interface IUserSessionService
    {
        Task RefreshUserClaimsAsync(ApplicationUser user);
        Task InvalidateUserSessionsAsync(ApplicationUser user);
        Task<bool> IsUserCurrentlyLoggedInAsync(int userId);
    }

    public class UserSessionService : IUserSessionService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserSessionService> _logger;

        public UserSessionService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserSessionService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Refreshes claims for the current user if they match the updated user
        /// </summary>
        public async Task RefreshUserClaimsAsync(ApplicationUser user)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                return;

            var currentUserId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !int.TryParse(currentUserId, out var userId) || userId != user.Id)
                return;

            try
            {
                // Refresh the sign-in to get updated claims
                await _signInManager.RefreshSignInAsync(user);
                _logger.LogInformation("Refreshed claims for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing claims for user {UserId}", user.Id);
            }
        }

        /// <summary>
        /// Invalidates all sessions for a specific user by updating their security stamp
        /// </summary>
        public async Task InvalidateUserSessionsAsync(ApplicationUser user)
        {
            try
            {
                await _userManager.UpdateSecurityStampAsync(user);
                _logger.LogInformation("Invalidated all sessions for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating sessions for user {UserId}", user.Id);
            }
        }

        /// <summary>
        /// Checks if the specified user is currently logged in to this session
        /// </summary>
        public async Task<bool> IsUserCurrentlyLoggedInAsync(int userId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                return false;

            var currentUserId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(currentUserId, out var currentId) && currentId == userId;
        }
    }
}