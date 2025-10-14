using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityCoreCustomization.Areas.Identity.Models
{
    public class ChangePasswordModel
    {
        public string StatusMessage { get; set; }

        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [DataType(DataType.Password)]
        [Display(Name = "کلمه عبور کنونی")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [StringLength(100, ErrorMessage = "طول {0} میبایست بین {2} تا {1} کاراکتر باشد.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "کلمه عبور جدید")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تکرار کلمه عبور جدید")]
        [Compare("NewPassword", ErrorMessage = "مقدار وارد شده برای کلمه عبور جدید و تکرار کلمه عبور جدید یکسان نیست.")]
        public string ConfirmPassword { get; set; }
    }
}
