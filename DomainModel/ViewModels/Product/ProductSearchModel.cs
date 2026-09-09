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

        public int? PointsFrom { get; set; }

        public int? PointsTo { get; set; }

        public int? RegisteredFrom { get; set; }

        public int? RegisteredTo { get; set; }

        public int? RemainingFrom { get; set; }

        public int? RemainingTo { get; set; }

    }
}
