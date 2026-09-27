using Framework.Common;

namespace DomainModel.ViewModels.Reports
{
    public class ReportActivitySearchModel : PageModel
    {
        public string? SearchTerm { get; set; }
        public long? ProductID { get; set; }
        public string? UserID { get; set; }
        public long? WarrantyCardID { get; set; }
        public int? RewardRequestID { get; set; }
        public int? RewardCatalogID { get; set; }
        public string? ActivityType { get; set; }
        public string? RewardStatus { get; set; }
        public int? PointsFrom { get; set; }
        public int? PointsTo { get; set; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtcExclusive { get; set; }
        public string? FromJalali { get; set; }
        public string? ToJalali { get; set; }
    }
}
