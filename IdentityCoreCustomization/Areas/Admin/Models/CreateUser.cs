using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Areas.Admin.Models
{
    public class CreateUserModel
    {
        [Display(Name = "نام کاربر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [StringLength(256, ErrorMessage = "{0} نمی‌تواند بیشتر از {1} کاراکتر باشد")]
        [RegularExpression(@"^[a-zA-Z0-9_@.-]+$", ErrorMessage = "نام کاربری فقط می‌تواند شامل حروف انگلیسی، اعداد، و علائم @._- باشد")]
        public string Username { get; set; }

        [EmailAddress(ErrorMessage = "مقدار وارد شده برای {0} شبیه یک آدرس ایمیل نیست")]
        [Display(Name = "ایمیل")]
        [StringLength(256, ErrorMessage = "{0} نمی‌تواند بیشتر از {1} کاراکتر باشد")]
        public string Email { get; set; }

        [Display(Name = "شماره تلفن")]
        [Phone(ErrorMessage = "شماره تلفن وارد شده معتبر نیست")]
        [RegularExpression(@"^(\+98|0)?9\d{9}$", ErrorMessage = "شماره تلفن باید با فرمت 09xxxxxxxxx یا +989xxxxxxxxx باشد")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [StringLength(100, ErrorMessage = "طول {0} میبایست حداقل {2} و حداکثر {1} کاراکتر باشد.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "کلمه عبور")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تایید کلمه عبور")]
        [Compare("Password", ErrorMessage = "دو کلمه عبور وارد شده یکسان نیستند.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "ایمیل تأیید شده")]
        public bool EmailConfirmed { get; set; } = true;

        [Display(Name = "شماره تلفن تأیید شده")]
        public bool PhoneNumberConfirmed { get; set; } = false;

        [Display(Name = "قابلیت قفل شدن")]
        public bool LockoutEnabled { get; set; } = true;

        [Display(Name = "فعال سازی دو مرحله‌ای")]
        public bool TwoFactorEnabled { get; set; } = false;
    }
}
