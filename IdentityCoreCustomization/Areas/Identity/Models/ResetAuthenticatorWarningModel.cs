using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Areas.Identity.Models
{
    public class ResetAuthenticatorWarningModel
    {
        [Display(Name = "پیام وضعیت")]
        public string StatusMessage { get; set; }
    }
}
