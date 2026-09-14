using Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.Warranty
{
    public class WarrantyCardSearchModel : PageModel
    {
        public string? SearchTerm { get; set; }
        public long? ProductID { get; set; }
        public bool? IsRegistered { get; set; }
        public string? ValidityPreset { get; set; }
        public int? RemainingDaysFrom { get; set; }
        public int? RemainingDaysTo { get; set; }
        public DateTime? RegisteredFrom { get; set; }
        public DateTime? RegisteredTo { get; set; }
        public string? RegisteredFromJalali { get; set; }
        public string? RegisteredToJalali { get; set; }
    }
}
