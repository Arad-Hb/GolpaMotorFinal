using Framework.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.Product
{
    public class ProductSearchModel : PageModel
    {
        public long ProductID { get; set; }

        [StringLength(50, ErrorMessage = "نام محصول باید حداکثر ۵۰ کاراکتر باشد.")]
        [Display(Name = "نام محصول")]
        public string? ProductName { get; set; }

        [Display(Name = "لینک تصویر پیش‌فرض")]
        public string? ImageUrl { get; set; }

        [Display(Name = "امتیاز محصول")]
        public int ProductPoint { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsAvailable { get; set; }

    }
}
