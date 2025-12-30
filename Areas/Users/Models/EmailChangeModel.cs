using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityCoreCustomization.Areas.Users.Models
{
    public class EmailChangeModel
    {
        [Display(Name = "نام کاربر")]
        public string Username { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "تاییده شده؟")]
        public bool IsEmailConfirmed { get; set; }

        public string StatusMessage { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "ایمیل جدید")]
        public string NewEmail { get; set; }
    }
}
