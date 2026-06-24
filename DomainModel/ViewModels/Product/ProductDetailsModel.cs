using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.Product
{
    public class ProductDetailsModel
    {
        public long ProductID { get; set; }

        [Display(Name = "نام محصول")]
        public string ProductName { get; set; } = null!;

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "تصویر")]
        public string? ImageUrl { get; set; }

        [Display(Name = "امتیاز محصول")]
        public int ProductPoint { get; set; }

        [Display(Name = "وضعیت موجودی")]
        public bool IsAvailable { get; set; }

        // برای صفحه جزئیات مهمه (ارتباط با گارانتی‌ها)
        public int WarrantyCount { get; set; }
    }
}
