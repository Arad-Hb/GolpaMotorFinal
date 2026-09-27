namespace DomainModel.ViewModels.Reports
{
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
}
