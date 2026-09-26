namespace DomainModel.Models
{
    public class ReportActivityLog
    {
        public long ReportActivityLogID { get; set; }
        public string SourceKey { get; set; } = null!;
        public string ActivityType { get; set; } = null!;
        public DateTime OccurredAtUtc { get; set; }
        public string UserID { get; set; } = null!;
        public long? ProductID { get; set; }
        public long? WarrantyCardID { get; set; }
        public int? CardRegistrationID { get; set; }
        public int? RewardRequestID { get; set; }
        public int? PointTransactionID { get; set; }
        public int PointsDelta { get; set; }
        public int TotalEarnedPoints { get; set; }
        public int TotalSettledPoints { get; set; }
        public int RemainedPoints { get; set; }
        public int AvailablePoints { get; set; }
        public string? StatusTitle { get; set; }
        public string? Description { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Product? Product { get; set; }
        public virtual WarrantyCard? WarrantyCard { get; set; }
        public virtual CardRegistration? CardRegistration { get; set; }
        public virtual RewardRequest? RewardRequest { get; set; }
        public virtual PointTransaction? PointTransaction { get; set; }
    }
}
