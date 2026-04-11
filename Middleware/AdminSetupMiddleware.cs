using System;
using System.Threading.Tasks;
using IdentityCoreCustomization.Models.Identity;
using IdentityCoreCustomization.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace IdentityCoreCustomization.Middleware
{
    /// <summary>
    /// Redirects all requests to /Admin/Setup when no Admin user exists yet.
    /// Uses method injection so the scoped UserManager is resolved per request.
    /// </summary>
    public sealed class AdminSetupMiddleware(RequestDelegate next)
    {
        private const string SetupPathPrefix = "/Admin/Setup";

        public async Task InvokeAsync(
            HttpContext context,
            AdminSetupState setupState,
            UserManager<ApplicationUser> userManager)
        {
            // Always allow the setup page and its static assets through
            if (context.Request.Path.StartsWithSegments(SetupPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            // On the first request after startup, check the DB and cache the result
            if (setupState.IsUnknown)
            {
                var admins = await userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count > 0)
                    setupState.MarkHasAdmin();
                else
                    setupState.MarkNoAdmin();
            }

            if (!setupState.HasAdmin)
            {
                context.Response.Redirect("/Admin/Setup");
                return;
            }

            await next(context);
        }
    }
}
