using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityCoreCustomization.Areas.Users.Models
{
    public class TwoFactorAuthenticationModel
    {
        [Display(Name="فعال بودن احراز هویت 2 مرحله ای")]
        public bool Is2faEnabled { get; set; }

        public bool CanEnable2fa { get; set; }

        [Display(Name = "دارای برنامه احراز هویت")]
        public bool HasAuthenticator { get; set; }

        [Display(Name = "تعداد کدهای بازیابی باقیمانده")]
        public int RecoveryCodesLeft { get; set; }

        [Display(Name = "این دستگاه به خاطر سپرده شده است")]
        public bool IsMachineRemembered { get; set; }

        [Display(Name = "شماره موبایل")]
        public string PhoneNumber { get; set; }

        [Display(Name = "پیام وضعیت")]
        public string StatusMessage { get; set; }
    }
}
