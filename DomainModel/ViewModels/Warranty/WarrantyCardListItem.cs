using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.Warranty
{
    public class WarrantyCardListItem
    {
        public long WarrantyCardID { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string ScratchedCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public bool IsRegistered { get; set; }
        public int ValidityMonths { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public int? RemainingDays { get; set; }
        public string RemainingText { get; set; } = string.Empty;
    }
}
