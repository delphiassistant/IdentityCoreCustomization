using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Models.Identity
{
    public class UserPhoneTokenConfirmViewModel
    {
        [Display(Name = "کلید شناسایی")]
        public string AuthenticationKey { get; set; }

        [Display(Name = "کد امنیتی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string AuthenticationCode { get; set; }
    }
}
