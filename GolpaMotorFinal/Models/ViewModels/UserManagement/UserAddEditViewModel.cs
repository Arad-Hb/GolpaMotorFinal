using System.ComponentModel.DataAnnotations;

namespace GolpaMotorFinal.Models.ViewModels.UserManagement
{
    public class UserAddEditViewModel
    {
        public string? UserID { get; set; }

        [Display(Name = "نام")]
        [StringLength(50)]
        public string? FirstName { get; set; }
        [Display(Name = "نام")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [StringLength(50)]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "نام کاربری اجباری است.")]
        [Display(Name = "نام کاربری")]
        public string? UserName { get; set; }

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
        public string? Address { get; set; }

        [Display(Name = "کد پستی")]
        public string? PostalCode { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "حذف شده")]
        public bool IsDeleted { get; set; }

        // فقط هنگام ایجاد کاربر اجباری
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string? Password { get; set; }

        // عکس جدید
        public IFormFile? ProfileImage { get; set; }

        // عکس فعلی
        public string? ExistingProfileImageUrl { get; set; }
    }
}
