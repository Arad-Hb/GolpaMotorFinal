using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel.ViewModels.User
{
    public class UserAddEditModel
    {
        public string? UserID { get; set; }

        [StringLength(50, MinimumLength = 2, ErrorMessage = "نام باید بین 2 تا 50 کاراکتر باشد.")]
        [Display(Name = "نام")]
        public string? FirstName { get; set; }

        [StringLength(50, MinimumLength = 2, ErrorMessage = "نام خانوادگی باید بین 2 تا 50 کاراکتر باشد.")]
        [Display(Name = "نام خانوادگی")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "ایمیل اجباری است.")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
        [Display(Name = "ایمیل")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "شماره موبایل اجباری است.")]
        [Phone(ErrorMessage = "شماره موبایل معتبر نیست.")]
        [Display(Name = "شماره موبایل")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "استان")]
        public int? ProvinceID { get; set; }

        [Display(Name = "شهر")]
        public int? CityID { get; set; }

        [StringLength(500)]
        [Display(Name = "آدرس")]
        public string? Address { get; set; }

        [StringLength(20)]
        [Display(Name = "کد پستی")]
        public string? PostalCode { get; set; }

        [Display(Name = "تصویر پروفایل")]
        public string? ProfileImageUrl { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "حذف شده")]
        public bool IsDeleted { get; set; }

        [Display(Name = "شماره کارت")]
        public string? CreditCartNumber { get; set; }

        [Display(Name = "شماره شبا")]
        public string? IBAN { get; set; }

        [Display(Name = "شماره حساب")]
        public string? AccountNumber { get; set; }
    }
}
