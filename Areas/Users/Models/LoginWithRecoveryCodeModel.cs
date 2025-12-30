using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Areas.Users.Models
{
    public class LoginWithRecoveryCodeModel
    {
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [DataType(DataType.Text)]
        [Display(Name = "کد بازیابی")]
        public string RecoveryCode { get; set; }

        public string ReturnUrl { get; set; }
    }
}
