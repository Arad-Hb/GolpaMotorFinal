using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.Product
{
    public class ProductListItem
    {
        public long ProductID { get; set; }

        [Required(ErrorMessage = "نام محصول اجباری است.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام محصول باید بین ۳ تا ۵۰ کاراکتر باشد.")]
        [Display(Name = "نام محصول")]
        public string ProductName { get; set; }

        [Display(Name = "لینک تصویر پیش‌فرض")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "وارد کردن امتیاز محصول اجباری می باشد .")]
        [Display(Name = "امتیاز محصول")]
        public int ProductPoint { get; set; }

        [Required(ErrorMessage = "وارد کردن وضعیت در دسترس بودن محصول اجباری است.")]
        [Display(Name = "موجود می باشد")]
        public bool IsAvailable { get; set; }

    }
}
