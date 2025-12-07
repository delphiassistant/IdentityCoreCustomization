using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckBoxList.Core.Models;
using IdentityCoreCustomization.Areas.Admin.Models;
using IdentityCoreCustomization.Classes.Extensions;
using IdentityCoreCustomization.Data;
using IdentityCoreCustomization.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityCoreCustomization.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Re-enable to ensure proper security
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<ApplicationUser> userManager, 
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            ILogger<UsersController> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index(string q)
        {
            try
            {
                var usersQuery = _userManager.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var pattern = $"%{q.Trim()}%";
                    usersQuery = usersQuery.Where(u =>
                        EF.Functions.Like(u.UserName, pattern) ||
                        EF.Functions.Like(u.Email ?? "", pattern) ||
                        EF.Functions.Like(u.PhoneNumber ?? "", pattern));
                }

                var users = await usersQuery
                    .OrderBy(u => u.UserName)
                    .AsNoTracking()
                    .ToListAsync();
                
                ViewBag.Search = q;
                _logger.LogInformation("Retrieved {Count} users for admin view (query: {Query})", users.Count, q);
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users for index view");
                TempData["ErrorMessage"] = "خطا در بارگذاری فهرست کاربران.";
                return View(new List<ApplicationUser>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
                var rolesList = roles.Select(r => new CheckBoxItem(r.Id, r.Name, false, false)).ToList();
                ViewBag.RolesList = rolesList;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading roles for user creation");
                TempData["ErrorMessage"] = "خطا در بارگذاری گروه‌های کاربری.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserModel model, List<int> selectedRoles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if user already exists
                    var existingUser = await _userManager.FindByNameAsync(model.Username);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Username", "این نام کاربری قبلاً استفاده شده است.");
                        await LoadRolesForView(selectedRoles);
                        return View(model);
                    }

                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        var existingEmailUser = await _userManager.FindByEmailAsync(model.Email);
                        if (existingEmailUser != null)
                        {
                            ModelState.AddModelError("Email", "این ایمیل قبلاً استفاده شده است.");
                            await LoadRolesForView(selectedRoles);
                            return View(model);
                        }
                    }

                    var user = new ApplicationUser 
                    { 
                        UserName = model.Username.Trim(), 
                        Email = model.Email?.Trim(),
                        PhoneNumber = model.PhoneNumber?.Trim(),
                        EmailConfirmed = model.EmailConfirmed,
                        PhoneNumberConfirmed = model.PhoneNumberConfirmed,
                        LockoutEnabled = model.LockoutEnabled,
                        TwoFactorEnabled = model.TwoFactorEnabled
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    
                    if (result.Succeeded)
                    {
                        // Assign roles using UserManager (proper way)
                        if (selectedRoles?.Any() == true)
                        {
                            var roleNames = await _roleManager.Roles
                                .Where(r => selectedRoles.Contains(r.Id))
                                .Select(r => r.Name)
                                .ToListAsync();

                            foreach (var roleName in roleNames)
                            {
                                var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                                if (!roleResult.Succeeded)
                                {
                                    _logger.LogWarning("Failed to add user {UserId} to role {RoleName}: {Errors}", 
                                        user.Id, roleName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                                }
                            }
                        }
                        
                        _logger.LogInformation("User '{Username}' (ID: {UserId}) created successfully by admin {AdminUser}", 
                            user.UserName, user.Id, User.Identity.Name);
                        TempData["SuccessMessage"] = $"کاربر '{user.UserName}' با موفقیت ایجاد شد.";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    foreach (var error in result.Errors) 
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user '{Username}'", model.Username);
                    ModelState.AddModelError(string.Empty, "خطا در ایجاد کاربر. لطفاً مجدداً تلاش کنید.");
                }
            }
            
            await LoadRolesForView(selectedRoles);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound("شناسه کاربر مشخص نشده است.");
            }

            try
            {
                var user = await _userManager.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound($"کاربری با شناسه {id} یافت نشد.");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user details for {UserId}", id);
                TempData["ErrorMessage"] = "خطا در بارگذاری جزئیات کاربر.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound("شناسه کاربر مشخص نشده است.");
            }

            try
            {
                var user = await _userManager.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound($"کاربری با شناسه {id} یافت نشد.");
                }

                var userRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
                await LoadRolesForView(userRoleIds);

                var model = new EditUserModel()
                {
                    UserID = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    TwoFactorEnabled = user.TwoFactorEnabled
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user {UserId} for edit", id);
                TempData["ErrorMessage"] = "خطا در بارگذاری اطلاعات کاربر.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserModel model, List<int> selectedRoles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.Users
                        .Include(u => u.UserRoles)
                        .FirstOrDefaultAsync(u => u.Id == model.UserID);

                    if (user == null)
                    {
                        return NotFound($"کاربری با شناسه {model.UserID} یافت نشد.");
                    }

                    // Check for username conflicts (excluding current user)
                    if (user.UserName != model.Username)
                    {
                        var existingUser = await _userManager.FindByNameAsync(model.Username);
                        if (existingUser != null && existingUser.Id != user.Id)
                        {
                            ModelState.AddModelError("Username", "این نام کاربری قبلاً استفاده شده است.");
                            await LoadRolesForView(selectedRoles);
                            return View(model);
                        }
                    }

                    // Update basic user properties
                    user.UserName = model.Username.Trim();
                    user.Email = model.Email?.Trim();
                    user.PhoneNumber = model.PhoneNumber?.Trim();
                    user.EmailConfirmed = model.EmailConfirmed;
                    user.PhoneNumberConfirmed = model.PhoneNumberConfirmed;
                    user.LockoutEnabled = model.LockoutEnabled;
                    user.TwoFactorEnabled = model.TwoFactorEnabled;

                    var updateResult = await _userManager.UpdateAsync(user);
                    
                    if (!updateResult.Succeeded)
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        await LoadRolesForView(selectedRoles);
                        return View(model);
                    }

                    // Update roles using UserManager (proper way)
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var selectedRoleNames = await _roleManager.Roles
                        .Where(r => selectedRoles.Contains(r.Id))
                        .Select(r => r.Name)
                        .ToListAsync();

                    var rolesChanged = false;

                    // Remove roles that are no longer selected
                    var rolesToRemove = currentRoles.Except(selectedRoleNames).ToList();
                    if (rolesToRemove.Any())
                    {
                        var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                        if (removeResult.Succeeded)
                        {
                            rolesChanged = true;
                            _logger.LogInformation("Removed roles {Roles} from user {UserId}", 
                                string.Join(", ", rolesToRemove), user.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to remove roles from user {UserId}: {Errors}", 
                                user.Id, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                        }
                    }

                    // Add new roles
                    var rolesToAdd = selectedRoleNames.Except(currentRoles).ToList();
                    if (rolesToAdd.Any())
                    {
                        var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                        if (addResult.Succeeded)
                        {
                            rolesChanged = true;
                            _logger.LogInformation("Added roles {Roles} to user {UserId}", 
                                string.Join(", ", rolesToAdd), user.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to add roles to user {UserId}: {Errors}", 
                                user.Id, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                        }
                    }

                    // CRITICAL: Force user to re-authenticate if roles changed
                    if (rolesChanged)
                    {
                        await ForceUserSignOutAsync(user);
                        _logger.LogInformation("User {UserId} security stamp updated due to role changes - will be signed out on next request", user.Id);
                        TempData["SuccessMessage"] = $"کاربر '{user.UserName}' با موفقیت به‌روزرسانی شد. تغییرات نقش‌ها در ورود بعدی اعمال خواهد شد.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = $"کاربر '{user.UserName}' با موفقیت به‌روزرسانی شد.";
                    }

                    _logger.LogInformation("User '{Username}' (ID: {UserId}) updated successfully by admin {AdminUser}", 
                        user.UserName, user.Id, User.Identity.Name);
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user {UserId}", model.UserID);
                    ModelState.AddModelError(string.Empty, "خطا در به‌روزرسانی کاربر. لطفاً مجدداً تلاش کنید.");
                }
            }
            
            await LoadRolesForView(selectedRoles);
            return View(model);
        }

        /// <summary>
        /// Forces a user to sign out by updating their security stamp
        /// This invalidates all existing authentication cookies/tokens
        /// </summary>
        private async Task ForceUserSignOutAsync(ApplicationUser user)
        {
            try
            {
                // Update SecurityStamp - this invalidates all existing authentication cookies
                await _userManager.UpdateSecurityStampAsync(user);
                
                // Also remove any stored authentication tickets for this user
                var existingTickets = await _context.AuthenticationTickets
                    .Where(t => t.UserId == user.Id)
                    .ToListAsync();
                
                if (existingTickets.Any())
                {
                    _context.AuthenticationTickets.RemoveRange(existingTickets);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Removed {Count} authentication tickets for user {UserId}", 
                        existingTickets.Count, user.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forcing sign-out for user {UserId}", user.Id);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound("شناسه کاربر مشخص نشده است.");
            }

            try
            {
                var user = await _userManager.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound($"کاربری با شناسه {id} یافت نشد.");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId} for deletion", id);
                TempData["ErrorMessage"] = "خطا در بارگذاری اطلاعات کاربر.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent user {UserId}", id);
                    return NotFound($"کاربری با شناسه {id} یافت نشد.");
                }

                // Prevent deleting the current admin user
                if (user.UserName == User.Identity.Name)
                {
                    TempData["ErrorMessage"] = "نمی‌توان کاربر فعلی را حذف کرد.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User '{Username}' (ID: {UserId}) deleted successfully by admin {AdminUser}", 
                        user.UserName, id, User.Identity.Name);
                    TempData["SuccessMessage"] = $"کاربر '{user.UserName}' با موفقیت حذف شد.";
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to delete user {UserId}: {Errors}", id, errors);
                    TempData["ErrorMessage"] = "خطا در حذف کاربر: " + errors;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                TempData["ErrorMessage"] = "خطا در حذف کاربر. لطفاً مجدداً تلاش کنید.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ChangePassword(int? id)
        {
            if (id == null)
            {
                return NotFound("شناسه کاربر مشخص نشده است.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound($"کاربری با شناسه {id} یافت نشد.");
                }

                var model = new ChangePasswordByAdminModel()
                {
                    UserID = user.Id,
                    Username = user.UserName
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading change password view for user {UserId}", id);
                TempData["ErrorMessage"] = "خطا در بارگذاری صفحه تغییر کلمه عبور.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordByAdminModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(model.UserID.ToString());
                    if (user == null)
                    {
                        return NotFound($"کاربری با شناسه {model.UserID} یافت نشد.");
                    }

                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                    
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Password changed for user '{Username}' (ID: {UserId}) by admin {AdminUser}", 
                            user.UserName, user.Id, User.Identity.Name);
                        TempData["SuccessMessage"] = $"کلمه عبور کاربر '{user.UserName}' با موفقیت تغییر یافت.";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error changing password for user {UserId}", model.UserID);
                    ModelState.AddModelError(string.Empty, "خطا در تغییر کلمه عبور. لطفاً مجدداً تلاش کنید.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Users/ToggleLockout/{id:int}")]
        public async Task<IActionResult> ToggleLockout(int id)
        {
            _logger.LogInformation("ToggleLockout called for user ID: {UserId}", id);
            
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    TempData["ErrorMessage"] = $"کاربری با شناسه {id} یافت نشد.";
                    return RedirectToAction(nameof(Index));
                }

                if (user.UserName == User.Identity.Name)
                {
                    _logger.LogWarning("Admin {AdminUser} attempted to lock themselves", User.Identity.Name);
                    TempData["ErrorMessage"] = "نمی‌توان کاربر فعلی را قفل کرد.";
                    return RedirectToAction(nameof(Index));
                }

                IdentityResult result;
                if (await _userManager.IsLockedOutAsync(user))
                {
                    // Unlock user
                    _logger.LogInformation("Unlocking user '{Username}' (ID: {UserId})", user.UserName, id);
                    result = await _userManager.SetLockoutEndDateAsync(user, null);
                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);
                        
                        // Force user to re-authenticate to get updated lockout status
                        await ForceUserSignOutAsync(user);
                        
                        TempData["SuccessMessage"] = $"کاربر '{user.UserName}' با موفقیت از حالت قفل خارج شد.";
                        _logger.LogInformation("User '{Username}' (ID: {UserId}) unlocked by admin {AdminUser}", 
                            user.UserName, id, User.Identity.Name);
                    }
                }
                else
                {
                    // Lock user for 1 year (effectively permanent)
                    _logger.LogInformation("Locking user '{Username}' (ID: {UserId})", user.UserName, id);
                    result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(1));
                    if (result.Succeeded)
                    {
                        // Force user to sign out immediately
                        await ForceUserSignOutAsync(user);
                        
                        TempData["SuccessMessage"] = $"کاربر '{user.UserName}' با موفقیت قفل شد.";
                        _logger.LogInformation("User '{Username}' (ID: {UserId}) locked by admin {AdminUser}", 
                            user.UserName, id, User.Identity.Name);
                    }
                }

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    TempData["ErrorMessage"] = "خطا در تغییر وضعیت قفل کاربر: " + errors;
                    _logger.LogError("Failed to toggle lockout for user {UserId}: {Errors}", id, errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling lockout for user {UserId}", id);
                TempData["ErrorMessage"] = "خطا در تغییر وضعیت قفل کاربر. لطفاً مجدداً تلاش کنید.";
            }

            return RedirectToAction(nameof(Index));
        }

        
        // Helper method to load roles for view
        private async Task LoadRolesForView(List<int> selectedRoles = null)
        {
            try
            {
                var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
                var rolesList = roles.Select(r => new CheckBoxItem(
                    r.Id, 
                    r.Name, 
                    selectedRoles?.Contains(r.Id) ?? false, 
                    false)).ToList();
                ViewBag.RolesList = rolesList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading roles for view");
                ViewBag.RolesList = new List<CheckBoxItem>();
            }
        }
    }
}
