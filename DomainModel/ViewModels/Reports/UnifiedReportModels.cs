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

    public class ReportPage<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int RecordCount { get; set; }
        public int PageCount => PageSize <= 0
            ? 0
            : (int)Math.Ceiling(RecordCount / (double)PageSize);
    }

    public class ProductWarrantyReportRow
    {
        public long ProductID { get; set; }
        public string ProductName { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; }
        public int TotalCards { get; set; }
        public int RegisteredCards { get; set; }
        public int UnregisteredCards { get; set; }
        public int UniqueCustomers { get; set; }
        public int AwardedPoints { get; set; }
        public int CustomersWithRewardRequest { get; set; }
        public int CustomersWithoutRewardRequest { get; set; }
        public int RewardRequestCount { get; set; }
        public int RegistrantSettledPoints { get; set; }
        public int RegistrantRemainedPoints { get; set; }
    }

    public class ReportActivityItem
    {
        public long ReportActivityLogID { get; set; }
        public string ActivityType { get; set; } = null!;
        public DateTime OccurredAtUtc { get; set; }
        public string UserID { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public long? ProductID { get; set; }
        public string? ProductName { get; set; }
        public long? WarrantyCardID { get; set; }
        public string? SerialNumber { get; set; }
        public string? ScratchedCode { get; set; }
        public int? RewardRequestID { get; set; }
        public string? RewardTitle { get; set; }
        public string? StatusTitle { get; set; }
        public int PointsDelta { get; set; }
        public int TotalEarnedPoints { get; set; }
        public int TotalSettledPoints { get; set; }
        public int RemainedPoints { get; set; }
        public int AvailablePoints { get; set; }
        public string? Description { get; set; }
    }

    public class ProductCardDetailItem
    {
        public long WarrantyCardID { get; set; }
        public long ProductID { get; set; }
        public string ProductName { get; set; } = null!;
        public string SerialNumber { get; set; } = null!;
        public string ScratchedCode { get; set; } = null!;
        public DateTime IssuedAtUtc { get; set; }
        public DateTime ProductAssignedAtUtc { get; set; }
        public bool IsRegistered { get; set; }
        public string? UserID { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? RegisteredAtUtc { get; set; }
        public int AwardedPoints { get; set; }
        public int RewardRequestCount { get; set; }
    }
}
