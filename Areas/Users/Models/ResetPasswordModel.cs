using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityCoreCustomization.Areas.Users.Models
{
    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [StringLength(100, ErrorMessage = "طول {0} میبایست حداقل {2} و حداکثر {1} کاراکتر باشد.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تایید کلمه عبور")]
        [Compare("Password", ErrorMessage = "دو کلمه عبور وارد شده یکسان نیستند.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
