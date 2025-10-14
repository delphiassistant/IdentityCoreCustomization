using Microsoft.AspNetCore.Identity;

namespace IdentityCoreCustomization.Models.Identity;

public class ApplicationUserToken : IdentityUserToken<int>
{
    public virtual ApplicationUser User { get; set; }
}