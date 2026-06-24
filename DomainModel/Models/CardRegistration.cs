using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class CardRegistration
    {
        public int CardRegisterationID { get; set; }

        public string SerialNumber { get; set; } = null!;

        public string ScratchedCode { get; set; } = null!;

        public string CustomerPhoneNumber { get; set; } = null!;

        public string UserID { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public string? FaildMessage { get; set; }

        public string? SuccessMessage { get; set; }

        public bool IsApproved { get; set; }

        public long WarrantyCardID { get; set; }

        public int EarnedPionts { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;

        public virtual WarrantyCard WarrantyCard { get; set; } = null!;
    }
}
