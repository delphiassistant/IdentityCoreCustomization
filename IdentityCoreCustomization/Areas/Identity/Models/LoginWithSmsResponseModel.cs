using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Areas.Identity.Models
{
    public class LoginWithSmsResponseModel
    {
        [Display(Name = "کلید شناسایی")]
        public string AuthenticationKey { get; set; }

        [Display(Name = "کد امنیتی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string AuthenticationCode { get; set; }
    }
}