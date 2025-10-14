using System;
using System.Collections.Generic;
using System.Linq;
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
    [Authorize(Roles = "Admin")] // Secure the entire controller
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
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if role already exists
                    var existingRole = await _roleManager.FindByNameAsync(model.Name);
                    if (existingRole != null)
                    {
                        ModelState.AddModelError("Name", "این گروه کاربری قبلاً ثبت شده است.");
                        return View(model);
                    }

                    // Create new role - RoleManager will handle NormalizedName automatically
                    var newRole = new ApplicationRole
                    {
                        Name = model.Name
                        // NormalizedName will be set automatically by RoleManager
                    };

                    var result = await _roleManager.CreateAsync(newRole);
                    
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Role '{RoleName}' created successfully by user {UserId}", 
                            newRole.Name, User.Identity.Name);
                        TempData["SuccessMessage"] = $"گروه کاربری '{newRole.Name}' با موفقیت ایجاد شد.";
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
                    ModelState.AddModelError(string.Empty, "خطا در ایجاد گروه کاربری. لطفاً مجدداً تلاش کنید.");
                }
            }
            
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit role called with null id");
                return NotFound("شناسه گروه کاربری مشخص نشده است.");
            }

            try
            {
                var role = await _roleManager.Roles
                    .Include(r => r.UserRoles)
                    .FirstOrDefaultAsync(r => r.Id == id);
                
                if (role == null)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found", id);
                    return NotFound("گروه کاربری مورد نظر یافت نشد.");
                }

                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role {RoleId} for edit", id);
                TempData["ErrorMessage"] = "خطا در بارگذاری اطلاعات گروه کاربری.";
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
                return NotFound("عدم تطابق شناسه گروه کاربری.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRole = await _roleManager.FindByIdAsync(id.ToString());
                    if (existingRole == null)
                    {
                        _logger.LogWarning("Role with ID {RoleId} not found for edit", id);
                        return NotFound("گروه کاربری مورد نظر یافت نشد.");
                    }

                    // Check if new name conflicts with existing roles (excluding current role)
                    var conflictingRole = await _roleManager.Roles
                        .Where(r => r.Id != id && r.Name == role.Name)
                        .FirstOrDefaultAsync();
                    
                    if (conflictingRole != null)
                    {
                        ModelState.AddModelError("Name", "این نام گروه کاربری قبلاً استفاده شده است.");
                        // Reload the role with UserRoles for the view
                        role = await _roleManager.Roles
                            .Include(r => r.UserRoles)
                            .FirstOrDefaultAsync(r => r.Id == id);
                        return View(role);
                    }

                    // Update the role name - RoleManager will handle NormalizedName automatically
                    existingRole.Name = role.Name;

                    var result = await _roleManager.UpdateAsync(existingRole);
                    
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Role '{RoleName}' (ID: {RoleId}) updated successfully by user {UserId}", 
                            role.Name, id, User.Identity.Name);
                        TempData["SuccessMessage"] = $"گروه کاربری '{role.Name}' با موفقیت به‌روزرسانی شد.";
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
                    ModelState.AddModelError(string.Empty, "این گروه کاربری توسط کاربر دیگری تغییر یافته است. لطفاً صفحه را بازخوانی کنید.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating role {RoleId}", id);
                    ModelState.AddModelError(string.Empty, "خطا در به‌روزرسانی گروه کاربری. لطفاً مجدداً تلاش کنید.");
                }
            }
            
            return View(role);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound("شناسه گروه کاربری مشخص نشده است.");
            }

            try
            {
                var role = await _roleManager.Roles
                    .Include(r => r.UserRoles)
                        .ThenInclude(ur => ur.User)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (role == null)
                {
                    return NotFound("گروه کاربری مورد نظر یافت نشد.");
                }

                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role details for {RoleId}", id);
                TempData["ErrorMessage"] = "خطا در بارگذاری جزئیات گروه کاربری.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound("شناسه گروه کاربری مشخص نشده است.");
            }

            try
            {
                var role = await _roleManager.Roles
                    .Include(r => r.UserRoles)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (role == null)
                {
                    return NotFound("گروه کاربری مورد نظر یافت نشد.");
                }

                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role {RoleId} for deletion", id);
                TempData["ErrorMessage"] = "خطا در بارگذاری اطلاعات گروه کاربری.";
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
                    return NotFound("گروه کاربری مورد نظر یافت نشد.");
                }

                // Check if role has users assigned
                var usersInRole = await _roleManager.Roles
                    .Where(r => r.Id == id)
                    .SelectMany(r => r.UserRoles)
                    .CountAsync();

                if (usersInRole > 0)
                {
                    TempData["ErrorMessage"] = $"نمی‌توان گروه کاربری '{role.Name}' را حذف کرد زیرا {usersInRole} کاربر به آن اختصاص داده شده است.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Role '{RoleName}' (ID: {RoleId}) deleted successfully by user {UserId}", 
                        role.Name, id, User.Identity.Name);
                    TempData["SuccessMessage"] = $"گروه کاربری '{role.Name}' با موفقیت حذف شد.";
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to delete role {RoleId}: {Errors}", id, errors);
                    TempData["ErrorMessage"] = "خطا در حذف گروه کاربری: " + errors;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {RoleId}", id);
                TempData["ErrorMessage"] = "خطا در حذف گروه کاربری. لطفاً مجدداً تلاش کنید.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if role exists
        private async Task<bool> RoleExistsAsync(int id)
        {
            return await _roleManager.Roles.AnyAsync(r => r.Id == id);
        }
    }
}
