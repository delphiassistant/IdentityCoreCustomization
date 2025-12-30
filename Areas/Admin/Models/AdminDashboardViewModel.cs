using System;

namespace IdentityCoreCustomization.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalRoles { get; set; }
        public int LockedOutUsers { get; set; }
        public int TwoFactorEnabledUsers { get; set; }
        public int UnconfirmedEmails { get; set; }
        public int UnconfirmedPhones { get; set; }
        public int OnlineSessions { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
