using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Models
{
    public class WarrantyCard
    {
        public long WarrantyCardID { get; set; }

        public long ProductID { get; set; }

        public string SerialNumber { get; set; } = null!;

        public string ScratchedCode { get; set; } = null!;

        public int ValidityMonths { get; set; }

        public bool IsRegistered { get; set; }

        public string? Description { get; set; }

        public virtual Product Product { get; set; } = null!;

        public virtual ICollection<CardRegistration> CardRegistrations { get; set; }
            = new HashSet<CardRegistration>();
    }
}
