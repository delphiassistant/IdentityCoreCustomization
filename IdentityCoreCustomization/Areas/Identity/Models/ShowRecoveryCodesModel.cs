using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Areas.Identity.Models
{
    public class ShowRecoveryCodesModel
    {
        [Display(Name = "کدهای بازیابی")]
        public IEnumerable<string> RecoveryCodes { get; set; }

        [Display(Name = "پیام وضعیت")]
        public string StatusMessage { get; set; }
    }
}
