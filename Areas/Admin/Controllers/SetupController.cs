using System.Threading.Tasks;
using IdentityCoreCustomization.Areas.Admin.Models;
using IdentityCoreCustomization.Models.Identity;
using IdentityCoreCustomization.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IdentityCoreCustomization.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AllowAnonymous]
    public class SetupController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AdminSetupState setupState,
        ILogger<SetupController> logger) : Controller
    {
        public async Task<IActionResult> Index()
        {
            // If setup is already complete, redirect to admin dashboard
            if (await AnyAdminExistsAsync())
            {
                setupState.MarkHasAdmin();
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            return View(new SetupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SetupViewModel model)
        {
            // Double-check: another request may have completed setup concurrently
            if (await AnyAdminExistsAsync())
            {
                setupState.MarkHasAdmin();
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            var roleResult = await userManager.AddToRoleAsync(user, "Admin");
            if (!roleResult.Succeeded)
            {
                // Roll back the created user so setup can be retried cleanly
                await userManager.DeleteAsync(user);
                foreach (var error in roleResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                logger.LogError("Failed to assign Admin role to user {Username}: {Errors}",
                    model.Username, string.Join(", ", roleResult.Errors));
                return View(model);
            }

            // Mark setup complete before signing in
            setupState.MarkHasAdmin();
            logger.LogInformation("First admin account created for user {Username}", model.Username);

            await signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        private async Task<bool> AnyAdminExistsAsync()
        {
            var admins = await userManager.GetUsersInRoleAsync("Admin");
            return admins.Count > 0;
        }
    }
}
