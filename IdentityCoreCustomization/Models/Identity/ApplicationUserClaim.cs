using Microsoft.AspNetCore.Identity;

namespace IdentityCoreCustomization.Models.Identity;

public class ApplicationUserClaim : IdentityUserClaim<int>
{
    public virtual ApplicationUser User { get; set; }
}