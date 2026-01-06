using System.Linq;
using System.Threading.Tasks;
using IdentityCoreCustomization.Areas.Admin.Models;
using IdentityCoreCustomization.Data;
using IdentityCoreCustomization.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityCoreCustomization.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalUsers = await _db.Users.CountAsync(),
                TotalRoles = await _db.Roles.CountAsync(),
                LockedOutUsers = await _db.Users.CountAsync(u => u.LockoutEnd != null && u.LockoutEnd > System.DateTimeOffset.UtcNow),
                TwoFactorEnabledUsers = await _db.Users.CountAsync(u => u.TwoFactorEnabled),
                UnconfirmedEmails = await _db.Users.CountAsync(u => u.Email != null && !u.EmailConfirmed),
                UnconfirmedPhones = await _db.Users.CountAsync(u => u.PhoneNumber != null && !u.PhoneNumberConfirmed),
                OnlineSessions = await _db.AuthenticationTickets.CountAsync(t => t.Expires == null || t.Expires > System.DateTimeOffset.UtcNow)
            };

            return View(vm);
        }
    }
}
