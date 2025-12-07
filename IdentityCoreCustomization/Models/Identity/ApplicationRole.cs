using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IdentityCoreCustomization.Classes.Validation;
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

    [Display(Name = "نام نقش (انگلیسی)")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    [StringLength(256, MinimumLength = 2, ErrorMessage = "{0} باید بین {2} تا {1} کاراکتر باشد")]
    [EnglishAlphanumeric(allowSpaces: false)]
    [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9_]*$", ErrorMessage = "نام نقش باید با حرف انگلیسی شروع شود و فقط می‌تواند شامل حروف، اعداد و underscore باشد.")]
    public override string Name { get; set; }

    [Display(Name = "نام نرمال شده نقش")]
    public override string NormalizedName { get; set; }
}