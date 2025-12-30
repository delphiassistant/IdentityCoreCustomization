using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityCoreCustomization.Models
{
    public class ProductCategory
    {
        [Key]
        public int ProductCategoryID { get; set; }
        [Display(Name = "گروه کالا")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string ProductCategoryTitle { get; set; }
    }
}
