using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Areas.Admin.Models
{
    public class ChangePasswordByAdminModel
    {
        public int UserID { get; set; }

        [Display(Name = "نام کاربر")]
        public string Username { get; set; }

        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [StringLength(100, ErrorMessage = "طول {0} میبایست حداقل {2} و حداکثر {1} کاراکتر باشد.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "کلمه عبور")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تایید کلمه عبور")]
        [Compare("Password", ErrorMessage = "دو کلمه عبور وارد شده یکسان نیستند.")]
        public string ConfirmPassword { get; set; }
    }
}
