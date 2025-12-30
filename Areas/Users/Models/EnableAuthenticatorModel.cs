using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Areas.Users.Models
{
    public class EnableAuthenticatorModel
    {
        [Display(Name = "کلید مشترک")]
        public string SharedKey { get; set; }

        [Display(Name = "آدرس URI برای احراز هویت")]
        public string AuthenticatorUri { get; set; }

        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [StringLength(7, ErrorMessage = "{0} باید {1} کاراکتر باشد.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "کد تایید")]
        public string Code { get; set; }

        [Display(Name = "پیام وضعیت")]
        public string StatusMessage { get; set; }
    }
}
