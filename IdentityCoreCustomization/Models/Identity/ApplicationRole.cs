using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace IdentityCoreCustomization.Models.Identity;

public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() : base()
    {
        // Ensure ConcurrencyStamp is always initialized
        // This is crucial for optimistic concurrency control
        if (string.IsNullOrEmpty(ConcurrencyStamp))
        {
            ConcurrencyStamp = Guid.NewGuid().ToString();
        }
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
        // Ensure ConcurrencyStamp is always initialized
        if (string.IsNullOrEmpty(ConcurrencyStamp))
        {
            ConcurrencyStamp = Guid.NewGuid().ToString();
        }
    }

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; }

    [Display(Name = "نام گروه")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    [StringLength(256, ErrorMessage = "{0} نمی‌تواند بیشتر از {1} کاراکتر باشد")]
    public override string Name { get; set; }

    [Display(Name = "نام نرمال شده گروه")]
    public override string NormalizedName { get; set; }
}