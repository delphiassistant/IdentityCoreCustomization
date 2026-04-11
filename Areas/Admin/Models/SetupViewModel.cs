using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Areas.Admin.Models
{
    public class SetupViewModel
    {
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [Display(Name = "نام کاربری")]
        public string Username { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "آدرس ایمیل معتبر نیست")]
        [Display(Name = "ایمیل (اختیاری)")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "تکرار رمز عبور الزامی است")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "رمز عبور و تکرار آن یکسان نیستند")]
        [Display(Name = "تکرار رمز عبور")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
