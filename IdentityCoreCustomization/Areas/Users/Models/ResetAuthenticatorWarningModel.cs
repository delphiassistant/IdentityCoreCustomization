using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Areas.Users.Models
{
    public class ResetAuthenticatorWarningModel
    {
        [Display(Name = "پیام وضعیت")]
        public string StatusMessage { get; set; }
    }
}
