using GolpaMotorFinal.Models.ViewModels.CRUD;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GolpaMotorFinal.Models.ViewModels.UserManagement
{
    public class UserAddEditViewModel
    {
        public string? UserID { get; set; }

        [Display(Name = "نام")]
        [StringLength(50)]
        public string? FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [StringLength(50)]
        public string? LastName { get; set; }

        [Display(Name = "رمز عبور")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "شماره موبایل اجباری است.")]
        [Phone(ErrorMessage = "شماره موبایل معتبر نیست.")]
        [Display(Name = "شماره موبایل")]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [Display(Name = "ایمیل")]
        public string? Email { get; set; }

        [Display(Name = "استان")]
        public int? ProvinceID { get; set; }

        [Display(Name = "شهر")]
        public int? CityID { get; set; }

        [Display(Name = "آدرس")]
        [StringLength(500)]
        public string? Address { get; set; }

        [Display(Name = "کد پستی")]
        [StringLength(20)]
        public string? PostalCode { get; set; }

        [Display(Name = "شماره کارت")]
        [StringLength(20)]
        public string? CreditCartNumber { get; set; }

        [Display(Name = "شماره شبا")]
        [StringLength(50)]
        public string? IBAN { get; set; }

        [Display(Name = "شماره حساب")]
        [StringLength(50)]
        public string? AccountNumber { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "حذف شده")]
        public bool IsDeleted { get; set; }

        // عکس فعلی
        public string? ProfileImageUrl { get; set; }

        // عکس جدید
        public IFormFile? ProfileImage { get; set; }

        public IEnumerable<SelectListItem> Provinces { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> Cities { get; set; } = new List<SelectListItem>();

        public CrudFormViewModel CrudFormViewModel { get; set; } = new();

    }
}
