namespace DomainModel.ViewModels.Reports
{
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
}
