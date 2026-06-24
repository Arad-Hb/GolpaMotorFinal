using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public int? ProvinceID { get; set; }

        public int? CityID { get; set; }

        public string? Address { get; set; }

        public string? PostalCode { get; set; }

        public string? ProfileImageUrl { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted {  get; set; } //جدید 

        public bool IsConfirmedCode { get; set; }

        public DateTime RegisterDate { get; set; } = DateTime.Now;
        //مالی
        public string? CreditCartNumber { get; set; }

        public string? IBAN { get; set; }

        public string? AccountNumber { get; set; }
        //امتیاز
        public int? TotalEarnedPoints { get; set; }

        public int? TotalSettledPoints { get; set; }

        public int? RemainedPoints { get; set; }

        public int? TotalRegisteredCards { get; set; }
        // OTP
        public string? VerificationCodeHash { get; set; }

        public string? VerificationCodeSalt { get; set; }

        public DateTime? VerificationCodeExpiresAt { get; set; }

        public string? VerificationCodePurpose { get; set; }

        public int VerificationCodeAttempts { get; set; }

        public string? VerificationCodeDestination { get; set; }

        public virtual Province Province { get; set; } = null!;

        public virtual City City { get; set; } = null!;

        public virtual ICollection<UserCustomerType> UserCustomerTypes { get; set; }
        = new HashSet<UserCustomerType>();

        public virtual ICollection<CardRegistration> CardRegistrations { get; set; }
            = new HashSet<CardRegistration>();

        public virtual ICollection<RewardRequest> RewardRequests { get; set; }
            = new HashSet<RewardRequest>();
    }
}
