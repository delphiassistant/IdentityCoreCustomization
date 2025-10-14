using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityCoreCustomization.Areas.Identity.Models
{
    public class LoginModel
    {

        public string ReturnUrl { get; set; }
        public string ErrorMessage { get; set; }

        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        //[EmailAddress]
        [Display(Name = "نام کاربری")]
        public string Username { get; set; }

        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [DataType(DataType.Password)]
        [Display(Name = "کلمه عبور")]
        public string Password { get; set; }

        [Display(Name = "مشخصات من را به خاطر بسپار")]
        public bool RememberMe { get; set; }
    }
}
