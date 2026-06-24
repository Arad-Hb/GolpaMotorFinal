using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.User
{
    public class UserDetailsModel
    {
        public string UserID { get; set; } =string.Empty;

        // اطلاعات شخصی
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;
        public string? RoleName { get; set; }

        // آدرس
        public string? Province { get; set; }

        public string? City { get; set; }

        public string? Address { get; set; }

        public string? PostalCode { get; set; }

        // پروفایل
        public string? ProfileImageUrl { get; set; }

        // وضعیت
        public bool IsActive { get; set; }

        public DateTime RegisterDate { get; set; }
        public bool EmailConfirmed { get; set; }

        // مالی
        public string? CreditCartNumber { get; set; }

        public string? IBAN { get; set; }

        public string? AccountNumber { get; set; }

        // امتیازات
        public int? TotalEarnedPoints { get; set; }

        public int? TotalSettledPoints { get; set; }

        public int? RemainedPoints { get; set; }

        public int? TotalRegisteredCards { get; set; }
    }
}
