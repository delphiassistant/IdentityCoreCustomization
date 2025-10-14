using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using IdentityCoreCustomization.Models.Identity;
using IdentityCoreCustomization.Classes.Identity;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Authentication;

namespace IdentityCoreCustomization.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserSessionsController : Controller
    {
        private readonly DatabaseTicketStore _databaseTicketStore;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserSessionsController> _logger;

        public UserSessionsController(
            DatabaseTicketStore databaseTicketStore,
            UserManager<ApplicationUser> userManager,
            ILogger<UserSessionsController> logger)
        {
            _databaseTicketStore = databaseTicketStore;
            _userManager = userManager;
            _logger = logger;
        }

        private bool IsAjaxRequest()
        {
            var header = Request?.Headers?["X-Requested-With"].ToString();
            return string.Equals(header, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Display list of currently online users from database
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Clean up expired sessions first
                var expiredCount = await _databaseTicketStore.CleanupExpiredSessionsAsync();
                if (expiredCount > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} expired sessions", expiredCount);
                }

                var onlineUsers = await _databaseTicketStore.GetAllOnlineUsersAsync();
                var activeSessionCount = await _databaseTicketStore.GetActiveSessionCountAsync();

                ViewBag.ActiveSessionCount = activeSessionCount;
                ViewBag.OnlineUserCount = onlineUsers.Count;
                ViewBag.ExpiredSessionsCleanedUp = expiredCount;
                ViewBag.RefreshTime = DateTime.Now;

                _logger.LogInformation("Retrieved {Count} online users with {ActiveCount} active sessions", 
                    onlineUsers.Count, activeSessionCount);
                
                return View(onlineUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving online users");
                TempData["ErrorMessage"] = "خطا در بارگذاری فهرست کاربران آنلاین.";
                return View(new List<OnlineUserSession>());
            }
        }

        /// <summary>
        /// Get online users data as JSON (for AJAX refresh)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOnlineUsers()
        {
            try
            {
                var onlineUsers = await _databaseTicketStore.GetAllOnlineUsersAsync();
                var activeSessionCount = await _databaseTicketStore.GetActiveSessionCountAsync();

                return Json(new
                {
                    success = true,
                    users = onlineUsers.Select(u => new
                    {
                        userId = u.UserId,
                        userName = u.UserName,
                        email = u.Email ?? "بدون ایمیل",
                        phoneNumber = u.PhoneNumber ?? "بدون شماره تلفن",
                        loginTime = u.LoginTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        lastActivity = u.LastActivity.ToString("yyyy-MM-dd HH:mm:ss"),
                        sessionDuration = FormatDuration(u.SessionDuration),
                        timeSinceLastActivity = FormatDuration(u.TimeSinceLastActivity),
                        isActive = u.IsActive,
                        isExpired = u.IsExpired,
                        ticketId = u.TicketId
                    }),
                    onlineCount = onlineUsers.Count,
                    activeSessionCount,
                    refreshTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online users via AJAX");
                return Json(new { success = false, error = "خطا در بارگذاری داده‌ها" });
            }
        }

        /// <summary>
        /// Force logout a specific user by user ID
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceLogoutUser(int userId, string userName)
        {
            if (userId <= 0)
            {
                if (IsAjaxRequest()) return Json(new { success = false, message = "شناسه کاربر معتبر نیست." });
                TempData["ErrorMessage"] = "شناسه کاربر معتبر نیست.";
                return RedirectToAction(nameof(Index));
            }

            int currentUserId;
            var currentIdStr = _userManager.GetUserId(User);
            currentUserId = int.TryParse(currentIdStr, out var parsed) ? parsed : -1;

            try
            {
                var success = await _databaseTicketStore.ForceLogoutUserAsync(userId);
                if (success)
                {
                    _logger.LogInformation("Admin {AdminUser} forced logout for user {UserId} ({UserName})", 
                        User.Identity?.Name ?? "Unknown", userId, userName ?? "Unknown");

                    // If targeting current user, also sign out immediately
                    if (userId == currentUserId)
                    {
                        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                        if (IsAjaxRequest()) return Json(new { success = true, self = true, message = "شما از سیستم خارج شدید." });
                        return RedirectToAction("Login", "Account", new { area = "Identity" });
                    }

                    if (IsAjaxRequest()) return Json(new { success = true, message = "کاربر با موفقیت از سیستم خارج شد." });
                    TempData["SuccessMessage"] = $"کاربر '{userName}' با موفقیت از سیستم خارج شد.";
                }
                else
                {
                    if (IsAjaxRequest()) return Json(new { success = false, message = "کاربر در حال حاضر آنلاین نیست یا نشست فعالی ندارد." });
                    TempData["WarningMessage"] = $"کاربر '{userName}' در حال حاضر آنلاین نیست.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forcing logout for user {UserId}", userId);
                if (IsAjaxRequest()) return Json(new { success = false, message = "خطا در خروج اجباری کاربر از سیستم." });
                TempData["ErrorMessage"] = "خطا در خروج اجباری کاربر از سیستم.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Force logout a specific session by ticket ID
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceLogoutSession(int ticketId, string userName)
        {
            if (ticketId <= 0)
            {
                if (IsAjaxRequest()) return Json(new { success = false, message = "شناسه نشست معتبر نیست." });
                TempData["ErrorMessage"] = "شناسه نشست معتبر نیست.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var success = await _databaseTicketStore.ForceLogoutSessionAsync(ticketId);
                if (success)
                {
                    _logger.LogInformation("Admin {AdminUser} forced logout for session {TicketId} (User: {UserName})", 
                        User.Identity.Name, ticketId, userName ?? "Unknown");

                    if (IsAjaxRequest()) return Json(new { success = true, message = "نشست کاربر با موفقیت قطع شد." });
                    TempData["SuccessMessage"] = $"نشست کاربر '{userName}' با موفقیت قطع شد.";
                }
                else
                {
                    if (IsAjaxRequest()) return Json(new { success = false, message = "نشست مورد نظر یافت نشد یا قبلاً قطع شده است." });
                    TempData["WarningMessage"] = "نشست مورد نظر یافت نشد یا قبلاً قطع شده است.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forcing logout for session {TicketId}", ticketId);
                if (IsAjaxRequest()) return Json(new { success = false, message = "خطا در قطع نشست." });
                TempData["ErrorMessage"] = "خطا در قطع نشست.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Clear all active sessions (nuclear option)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAllSessions()
        {
            try
            {
                var sessionCount = await _databaseTicketStore.ClearAllSessionsAsync();
                _logger.LogWarning("Admin {AdminUser} cleared all active sessions - {SessionCount} sessions terminated", 
                    User.Identity.Name, sessionCount);

                if (IsAjaxRequest()) return Json(new { success = true, message = "تمام نشست‌های فعال پاک شد.", count = sessionCount });
                TempData["SuccessMessage"] = $"تمام نشست‌های فعال ({sessionCount} نشست) پاک شد. توجه: شما نیز از سیستم خارج خواهید شد.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all sessions");
                if (IsAjaxRequest()) return Json(new { success = false, message = "خطا در پاک کردن نشست‌ها." });
                TempData["ErrorMessage"] = "خطا در پاک کردن نشست‌ها.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Get session details for a specific user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserSessions(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return Json(new { success = false, error = "شناسه کاربر معتبر نیست." });
                }

                var userSessions = await _databaseTicketStore.GetUserSessionsAsync(userId);
                
                return Json(new
                {
                    success = true,
                    sessions = userSessions.Select(s => new
                    {
                        ticketId = s.TicketId,
                        loginTime = s.LoginTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        lastActivity = s.LastActivity.ToString("yyyy-MM-dd HH:mm:ss"),
                        sessionDuration = FormatDuration(s.SessionDuration),
                        timeSinceLastActivity = FormatDuration(s.TimeSinceLastActivity),
                        isActive = s.IsActive,
                        isExpired = s.IsExpired
                    }),
                    count = userSessions.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sessions for user {UserId}", userId);
                return Json(new { success = false, error = "خطا در بارگذاری نشست‌های کاربر" });
            }
        }

        /// <summary>
        /// Check if a specific user is currently online
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckUserStatus(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return Json(new { success = false, error = "شناسه کاربر معتبر نیست." });
                }

                var isOnline = await _databaseTicketStore.IsUserOnlineAsync(userId);
                var user = await _userManager.FindByIdAsync(userId.ToString());
                
                return Json(new
                {
                    success = true,
                    userId,
                    userName = user?.UserName ?? "Unknown",
                    isOnline,
                    status = isOnline ? "آنلاین" : "آفلاین"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking status for user {UserId}", userId);
                return Json(new { success = false, error = "خطا در بررسی وضعیت کاربر" });
            }
        }

        /// <summary>
        /// Cleanup expired sessions manually
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanupExpiredSessions()
        {
            try
            {
                var expiredCount = await _databaseTicketStore.CleanupExpiredSessionsAsync();
                _logger.LogInformation("Admin {AdminUser} manually cleaned up {Count} expired sessions", 
                    User.Identity.Name, expiredCount);

                if (IsAjaxRequest()) return Json(new { success = true, message = "نشست‌های منقضی شده پاک شدند.", count = expiredCount });
                TempData["SuccessMessage"] = $"{expiredCount} نشست منقضی شده پاک شد.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired sessions");
                if (IsAjaxRequest()) return Json(new { success = false, message = "خطا در پاک کردن نشست‌های منقضی شده." });
                TempData["ErrorMessage"] = "خطا در پاک کردن نشست‌های منقضی شده.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Refresh online users view (AJAX endpoint)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RefreshOnlineUsers()
        {
            return await GetOnlineUsers();
        }

        /// <summary>
        /// Helper method to format duration
        /// </summary>
        private string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
                return $"{(int)duration.TotalDays} روز، {duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
            else if (duration.TotalHours >= 1)
                return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
            else
                return $"{duration.Minutes:D2}:{duration.Seconds:D2}";
        }
    }
}
