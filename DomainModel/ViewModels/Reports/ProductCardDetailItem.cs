namespace DomainModel.ViewModels.Reports
{
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
        public DateTime? PointsAwardedAtUtc { get; set; }
        public int ValidityMonths { get; set; }
        public int? RemainingDays { get; set; }
        public string RemainingValidityText { get; set; } = "شروع‌نشده";
        public int RewardRequestCount { get; set; }
    }
}
