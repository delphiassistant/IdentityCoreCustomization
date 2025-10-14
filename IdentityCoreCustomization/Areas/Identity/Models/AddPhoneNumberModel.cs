using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Areas.Identity.Models;

public class AddPhoneNumberModel
{
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    [Display(Name = "شماره موبایل")]
    public string PhoneNumber { get; set; }
}