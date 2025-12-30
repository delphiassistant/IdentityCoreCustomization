using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Areas.Users.Models
{
    public class LoginWith2faModel
    {
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [StringLength(7, ErrorMessage = "طول {0} باید بین {2} تا {1} کاراکتر باشد.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "کد احراز هویت")]
        public string TwoFactorCode { get; set; }

        [Display(Name = "به خاطر سپردن این دستگاه")]
        public bool RememberMachine { get; set; }

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}
