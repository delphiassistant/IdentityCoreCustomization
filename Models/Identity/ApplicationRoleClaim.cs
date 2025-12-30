using Microsoft.AspNetCore.Identity;

namespace IdentityCoreCustomization.Models.Identity;

public class ApplicationRoleClaim : IdentityRoleClaim<int>
{
    public virtual ApplicationRole Role { get; set; }
}