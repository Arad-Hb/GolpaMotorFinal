using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.User
{
    public class UserDetailsModel
    {
        public string UserID { get; set; }

        // اطلاعات شخصی
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public string RoleName { get; set; }

        public string ProfileImageUrl { get; set; }

        // آدرس
        public string Province { get; set; }

        public string City { get; set; }

        public string Address { get; set; }

        public string PostalCode { get; set; }

        // وضعیت
        public bool IsActive { get; set; }

        public DateTime RegisterDate { get; set; }

        // مالی
        public string CreditCartNumber { get; set; }

        public string IBAN { get; set; }

        public string AccountNumber { get; set; }

        // امتیازات
        public int TotalEarnedPoints { get; set; }

        public int TotalSettledPoints { get; set; }

        public int RemainedPoints { get; set; }

        public int TotalRegisteredCards { get; set; }
    }
}
