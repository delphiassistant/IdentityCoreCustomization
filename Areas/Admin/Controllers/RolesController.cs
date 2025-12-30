using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RolesController> _logger;

        public RolesController( 
            RoleManager<ApplicationRole> roleManager, 
            ApplicationDbContext context,
            ILogger<RolesController> logger)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .Include(r => r.UserRoles)
                    .OrderBy(r => r.Name)
                    .ToListAsync();
                
                _logger.LogInformation("Retrieved {Count} roles for admin view", roles.Count);
                return View(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles for index view");
                TempData["ErrorMessage"] = "خطا در بارگذاری فهرست گروه‌های کاربری.";
                return View(new List<ApplicationRole>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationRole model)
        {
            // Additional server-side validation for English-only role names
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                var validationError = ValidateRoleName(model.Name);
                if (validationError != null)
                {
                    ModelState.AddModelError("Name", validationError);
                    return View(model);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if role already exists
                    var existingRole = await _roleManager.FindByNameAsync(model.Name);
                    if (existingRole != null)
                    {
                        ModelState.AddModelError("Name", "این نقش قبلاً ثبت شده است.");
                        return View(model);
                    }

                    // Create new role - RoleManager will handle NormalizedName automatically
                    var newRole = new ApplicationRole
                    {
                        Name = model.Name.Trim()
                    };

                    var result = await _roleManager.CreateAsync(newRole);
                    
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Role '{RoleName}' created successfully by user {UserId}", 
                            newRole.Name, User.Identity.Name);
                        TempData["SuccessMessage"] = $"نقش '{newRole.Name}' با موفقیت ایجاد شد.";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    // Add Identity errors to ModelState
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating role '{RoleName}'", model.Name);
                    ModelState.AddModelError(string.Empty, "خطا در ایجاد نقش. لطفاً مجدداً تلاش کنید.");
                }
            }
            
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit role called with null id");
                return NotFound("شناسه نقش مشخص نشده است.");
            }

            try
            {
                var role = await _roleManager.Roles
                    .Include(r => r.UserRoles)
                    .FirstOrDefaultAsync(r => r.Id == id);
                
                if (role == null)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found", id);
                    return NotFound("نقش مورد نظر یافت نشد.");
                }

                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role {RoleId} for edit", id);
                TempData["ErrorMessage"] = "خطا در بارگذاری اطلاعات نقش.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ApplicationRole role)
        {
            if (id != role.Id)
            {
                _logger.LogWarning("Role ID mismatch: URL ID {UrlId}, Model ID {ModelId}", id, role.Id);
                return NotFound("عدم تطابق شناسه نقش.");
            }

            // Additional server-side validation for English-only role names
            if (!string.IsNullOrWhiteSpace(role.Name))
            {
                var validationError = ValidateRoleName(role.Name);
                if (validationError != null)
                {
                    ModelState.AddModelError("Name", validationError);
                    // Reload the role with UserRoles for the view
                    role = await _roleManager.Roles
                        .Include(r => r.UserRoles)
                        .FirstOrDefaultAsync(r => r.Id == id);
                    return View(role);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRole = await _roleManager.FindByIdAsync(id.ToString());
                    if (existingRole == null)
                    {
                        _logger.LogWarning("Role with ID {RoleId} not found for edit", id);
                        return NotFound("نقش مورد نظر یافت نشد.");
                    }

                    // Check if new name conflicts with existing roles (excluding current role)
                    var conflictingRole = await _roleManager.Roles
                        .Where(r => r.Id != id && r.Name == role.Name)
                        .FirstOrDefaultAsync();
                    
                    if (conflictingRole != null)
                    {
                        ModelState.AddModelError("Name", "این نام نقش قبلاً استفاده شده است.");
                        role = await _roleManager.Roles
                            .Include(r => r.UserRoles)
                            .FirstOrDefaultAsync(r => r.Id == id);
                        return View(role);
                    }

                    // Log warning about role name change
                    if (existingRole.Name != role.Name.Trim())
                    {
                        _logger.LogWarning(
                            "Role name being changed from '{OldName}' to '{NewName}' by user {UserId}. This may affect access control.",
                            existingRole.Name, role.Name.Trim(), User.Identity.Name);
                    }

                    // Update the role name
                    existingRole.Name = role.Name.Trim();

                    var result = await _roleManager.UpdateAsync(existingRole);
                    
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Role '{RoleName}' (ID: {RoleId}) updated successfully by user {UserId}", 
                            role.Name, id, User.Identity.Name);
                        TempData["SuccessMessage"] = $"نقش '{role.Name}' با موفقیت به‌روزرسانی شد.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Add Identity errors to ModelState
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency conflict when updating role {RoleId}", id);
                    ModelState.AddModelError(string.Empty, "این نقش توسط کاربر دیگری تغییر یافته است. لطفاً صفحه را بازخوانی کنید.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating role {RoleId}", id);
                    ModelState.AddModelError(string.Empty, "خطا در به‌روزرسانی نقش. لطفاً مجدداً تلاش کنید.");
                }
            }
            
            return View(role);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound("شناسه نقش مشخص نشده است.");
            }

            try
            {
                var role = await _roleManager.Roles
                    .Include(r => r.UserRoles)
                        .ThenInclude(ur => ur.User)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (role == null)
                {
                    return NotFound("نقش مورد نظر یافت نشد.");
                }

                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role details for {RoleId}", id);
                TempData["ErrorMessage"] = "خطا در بارگذاری جزئیات نقش.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound("شناسه نقش مشخص نشده است.");
            }

            try
            {
                var role = await _roleManager.Roles
                    .Include(r => r.UserRoles)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (role == null)
                {
                    return NotFound("نقش مورد نظر یافت نشد.");
                }

                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role {RoleId} for deletion", id);
                TempData["ErrorMessage"] = "خطا در بارگذاری اطلاعات نقش.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent role {RoleId}", id);
                    return NotFound("نقش مورد نظر یافت نشد.");
                }

                // Check if role has users assigned
                var usersInRole = await _roleManager.Roles
                    .Where(r => r.Id == id)
                    .SelectMany(r => r.UserRoles)
                    .CountAsync();

                if (usersInRole > 0)
                {
                    TempData["ErrorMessage"] = $"نمی‌توان نقش '{role.Name}' را حذف کرد زیرا {usersInRole} کاربر به آن اختصاص داده شده است.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Role '{RoleName}' (ID: {RoleId}) deleted successfully by user {UserId}", 
                        role.Name, id, User.Identity.Name);
                    TempData["SuccessMessage"] = $"نقش '{role.Name}' با موفقیت حذف شد.";
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to delete role {RoleId}: {Errors}", id, errors);
                    TempData["ErrorMessage"] = "خطا در حذف نقش: " + errors;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {RoleId}", id);
                TempData["ErrorMessage"] = "خطا در حذف نقش. لطفاً مجدداً تلاش کنید.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Validates role name for English-only alphanumeric characters
        /// </summary>
        private string ValidateRoleName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return null;
            }

            roleName = roleName.Trim();

            // Check for Persian/Arabic characters
            if (Regex.IsMatch(roleName, @"[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF]"))
            {
                return "نام نقش نمی‌تواند شامل حروف فارسی یا عربی باشد. فقط از حروف انگلیسی استفاده کنید.";
            }

            // Check for spaces
            if (roleName.Contains(" "))
            {
                return "نام نقش نمی‌تواند شامل فاصله باشد. از underscore (_) یا camelCase استفاده کنید.";
            }

            // Check for special characters (except underscore)
            if (Regex.IsMatch(roleName, @"[^a-zA-Z0-9_]"))
            {
                return "نام نقش فقط می‌تواند شامل حروف انگلیسی، اعداد و underscore (_) باشد.";
            }

            // Check if starts with a letter
            if (!Regex.IsMatch(roleName, @"^[a-zA-Z]"))
            {
                return "نام نقش باید با یک حرف انگلیسی شروع شود.";
            }

            // Check minimum length
            if (roleName.Length < 2)
            {
                return "نام نقش باید حداقل 2 کاراکتر باشد.";
            }

            return null; // Valid
        }

        // Helper method to check if role exists
        private async Task<bool> RoleExistsAsync(int id)
        {
            return await _roleManager.Roles.AnyAsync(r => r.Id == id);
        }
    }
}
