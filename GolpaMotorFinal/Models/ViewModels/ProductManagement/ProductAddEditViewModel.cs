using DomainModel.Models;
using System.ComponentModel.DataAnnotations;

namespace GolpaMotorFinal.Models.ViewModels.ProductManagement
{
    public class ProductAddEditViewModel
    {
        public long ProductID { get; set; }

        [Required(ErrorMessage = "نام محصول اجباری است.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام محصول باید بین ۳ تا ۵۰ کاراکتر باشد.")]
        [Display(Name = "نام محصول")]
        public string ProductName { get; set; }

        [StringLength(500, MinimumLength = 3, ErrorMessage = "توضیحات باید بین ۳ تا ۵۰۰ کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "وارد کردن امتیاز محصول اجباری می باشد .")]
        [Display(Name = "امتیاز محصول")]
        //[Range(0, 5)]
        public int ProductPoint { get; set; }

        [Required(ErrorMessage = "وارد کردن وضعیت در دسترس بودن محصول اجباری است.")]
        [Display(Name = "موجود می باشد")]
        public bool IsAvailable { get; set; }

        // تصویر جدید
        public IFormFile? ImageFile { get; set; }

        // تصویر قبلی (برای Update)
        public string? ExistingImageUrl { get; set; }
    }
}
