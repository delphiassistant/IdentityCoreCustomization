using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using IdentityCoreCustomization.Models.Identity;
using IdentityCoreCustomization.Data;

namespace IdentityCoreCustomization.Services
{
    public static class DatabaseSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseSeeder");
            

            try
            {
                // Ensure Admin role exists
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    var adminRole = new ApplicationRole("Admin");
                    var result = await roleManager.CreateAsync(adminRole);
                    
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Admin role created successfully with ConcurrencyStamp: {ConcurrencyStamp}", adminRole.ConcurrencyStamp);
                    }
                    else
                    {
                        logger.LogError("Failed to create Admin role: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while seeding roles");
            }
        }
    }
}