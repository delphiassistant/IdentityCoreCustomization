using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Models.Identity
{
    public class UserPhoneToken
    {
        [Key]
        public int UserPhoneTokenID { get; set; }
        
        [Display(Name ="شماره موبایل")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [RegularExpression(@"09\d{9}",ErrorMessage = "قالب مقدار وارد شده برای {0} صحیح نیست.")]
        public string PhoneNumber { get; set; }

        public bool Confirmed { get; set; }

        [Display(Name = "تاریخ انقضای کد")]
        public DateTime ExpireTime { get; set; }

        [Display(Name = "کد امنیتی")]
        public string AuthenticationCode { get; set; }

        [Display(Name = "کلید شناسایی")]
        public string AuthenticationKey { get; set; }

        public string GenerateNewAuthenticationCode()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString();
        }

        public string GenerateNewAuthenticationKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public void Initialize()
        {
            AuthenticationCode = GenerateNewAuthenticationCode();
            AuthenticationKey = GenerateNewAuthenticationKey();
        }
    }
}
