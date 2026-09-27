using Framework.Common;

namespace DomainModel.ViewModels.Reports
{
    public class ProductWarrantyReportSearchModel : PageModel
    {
        public long? ProductID { get; set; }
        public string? SearchTerm { get; set; }
        public bool? IsRegistered { get; set; }
        public int? CountFrom { get; set; }
        public int? CountTo { get; set; }
        public int? PointsFrom { get; set; }
        public int? PointsTo { get; set; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtcExclusive { get; set; }
        public string? FromJalali { get; set; }
        public string? ToJalali { get; set; }
    }
}
