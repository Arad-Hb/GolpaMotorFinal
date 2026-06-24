using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GolpaMotorFinal.Models.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "لطفاً ایمیل را وارد کنید.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست.")]
        [Display(Name = "آدرس ایمیل")]
        public string Email { get; set; }


        [Required(ErrorMessage = "لطفاً رمز عبور را وارد کنید.")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [Required(ErrorMessage = "لطفاً تکرار رمز عبور را وارد کنید.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "رمز عبور و تکرار آن یکسان نیست.")]
        [Display(Name = "تکرار رمز عبور")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "لطفاً نام خود را وارد کنید.")]
        [MaxLength(60, ErrorMessage = "نام نمی‌تواند بیشتر از 60 کاراکتر باشد.")]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "لطفاً نام خانوادگی خود را وارد کنید.")]
        [MaxLength(60, ErrorMessage = "نام خانوادگی نمی‌تواند بیشتر از 60 کاراکتر باشد.")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }


        [Required(ErrorMessage = "لطفاً شماره موبایل را وارد کنید.")]
        [StringLength(15,MinimumLength = 10,ErrorMessage = "شماره موبایل باید بین 10 تا 15 رقم باشد.")]
        [Display(Name = "موبایل")]
        public string PhoneNumber { get; set; }

        [Display(Name = "استان")]
        public int? ProvinceID { get; set; }

        [Display(Name = "شهر")]
        public int? CityID { get; set; }

        public IEnumerable<SelectListItem>? Provinces { get; set; }

        public IEnumerable<SelectListItem>? Cities { get; set; }

        public IEnumerable<SelectListItem>? Roles { get; set; }

        public string? ReturnUrl { get; internal set; }
    }
}

