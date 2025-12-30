using Microsoft.AspNetCore.Identity;

namespace IdentityCoreCustomization.Models.Identity;

public class ApplicationUserLogin : IdentityUserLogin<int>
{
    public virtual ApplicationUser User { get; set; }
}