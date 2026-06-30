using DomainModel.Models;
using System.ComponentModel.DataAnnotations;

namespace GolpaMotorFinal.Models.ViewModels.UserManagement
{
    public class UserReportViewModel
    {
        public string UserID { get; set; }

        [Display(Name = "نام")]
        public string FullName { get; set; }

        [Display(Name = "موبایل")]
        public string PhoneNumber { get; set; }

        [Display(Name = "شغل")]
        public string RoleName { get; set; }

        [Display(Name = "کارت ثبت شده")]
        public int? TotalRegisteredCards { get; set; }

        [Display(Name = "امتیاز کسب شده")]
        public int? TotalEarnedPoints { get; set; }

        [Display(Name = "امتیاز تسویه شده")]
        public int? TotalSettledPoints { get; set; }

        [Display(Name = "امتیاز باقیمانده")]
        public int? RemainedPoints { get; set; }

        [Display(Name = "تصویر پروفایل")]
        public string ProfileImageUrl { get; set; }

        [Display(Name = "استان")]
        public string Province { get; set; }

        [Display(Name = "شهر")]
        public string City { get; set; } 

    }
}
