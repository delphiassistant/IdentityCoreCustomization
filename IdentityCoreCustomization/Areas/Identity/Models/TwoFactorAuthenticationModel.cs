using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityCoreCustomization.Areas.Identity.Models
{
    public class TwoFactorAuthenticationModel
    {

        [Display(Name="فعال بودن احراز هویت 2 مرحله ای")]
        public bool Is2faEnabled { get; set; }

        public bool CanEnable2fa { get; set; }

        [Display(Name = "پیام وضعیت")]
        public string StatusMessage { get; set; }
    }
}
