using System.ComponentModel.DataAnnotations;

namespace GolpaMotorFinal.Models.ViewModels.UserManagement
{
    public class UserListItemViewModel
    {
        [Display(Name = "شناسه")]
        public string UserID { get; set; }

        [Display(Name = "نام")]
        public string FullName { get; set; }

        [Display(Name = "عکس پروفایل")]
        public string ProfileImageUrl { get; set; }

        [Display(Name = "موبایل")]
        public string PhoneNumber { get; set; }

        [Display(Name = "شغل")]
        public string RoleName { get; set; }

        [Display(Name = "کارت ثبت شده")]
        public int? TotalRegisteredCards { get; set; }

        [Display(Name = "امتیاز کسب شده")]
        public int? TotalEarnedPoints { get; set; }

        [Display(Name = "استان")]
        public string Province { get; set; }

        [Display(Name = "شهر")]
        public string City { get; set; }

    }
}
