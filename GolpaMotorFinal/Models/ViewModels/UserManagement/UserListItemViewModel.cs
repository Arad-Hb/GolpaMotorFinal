using System.ComponentModel.DataAnnotations;

namespace GolpaMotorFinal.Models.ViewModels.UserManagement
{
    public class UserListItemViewModel
    {
        [Display(Name = "شناسه")]
        public string UserId { get; set; }


        [Required(ErrorMessage = "لطفاً نام خود را وارد کنید.")]
        [MaxLength(60)]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "لطفاً نام خانوادگی خود را وارد کنید.")]
        [MaxLength(60)]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [RegularExpression(
            @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
            ErrorMessage = "Invalid email address.")]
        [StringLength(14, MinimumLength = 12)]
        [Display(Name = "ایمیل")]
        public string EmailAddress { get; set; } = null;

        [RegularExpression("/^(\\+\\d{1,3}[- ]?)?\\d{10}$/")]
        [StringLength(14, MinimumLength = 12)]
        [Display(Name = "موبایل")]
        public string PhoneNumber { get; set; } = null!;

        [Display(Name = "تاریخ ثبت نام")]
        public DateTime RegisterDate { get; set; }

        [Display(Name = "استان")]
        public string Province { get; set; }

        [Display(Name = "شهر")]
        public string City { get; set; }
    }
}
