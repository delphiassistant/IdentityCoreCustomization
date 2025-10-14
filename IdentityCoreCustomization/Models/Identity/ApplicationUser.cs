using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityCoreCustomization.Models.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        // Additional Properties
        public virtual ICollection<ApplicationUserClaim> Claims { get; set; }
        public virtual ICollection<ApplicationUserLogin> Logins { get; set; }
        public virtual ICollection<ApplicationUserToken> Tokens { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
        //public virtual ICollection<Order> Orders { get; set; }
        
        
        [Display(Name= "نام کاربر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public override string UserName { get; set; }

        [Display(Name = "نام کاربر - نرمال شده")]
        public override string NormalizedUserName { get; set; }

        [Display(Name = "ایمیل")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public override string Email { get; set; }
        [Display(Name = "ایمیل - نرمال شده")]
        public override string NormalizedEmail { get; set; }

        [Display(Name = "ایمیل تایید شده؟")]
        public override bool EmailConfirmed { get; set; }

        [Display(Name = "شماره موبایل")]
        public override string PhoneNumber { get; set; }

        [Display(Name = "شماره موبایل تایید شده؟")]
        public override bool PhoneNumberConfirmed { get; set; }

        [Display(Name = "احراز هویت دو مرحله ای فعال است؟")]
        public override bool TwoFactorEnabled { get; set; }

        [Display(Name = "تاریخ پایان قفل حساب کاربری")]
        public override DateTimeOffset? LockoutEnd { get; set; }

        [Display(Name = "فعال بودن قفل حساب کاربری")]
        public override bool LockoutEnabled { get; set; }

        [Display(Name = "دفعات تلاش ناموفق برای ورود به حساب کاربری")]
        public override int AccessFailedCount { get; set; }

    }
}
