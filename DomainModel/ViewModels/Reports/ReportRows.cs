namespace DomainModel.ViewModels.Reports
{
    public class WarrantyProductStatusRow
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalCards { get; set; }
        public int Registered { get; set; }
        public int Unregistered { get; set; }
        public int Expired { get; set; }
        public int ExpiringSoon { get; set; }
    }

    public class ProductPopularityRow
    {
        public string ProductName { get; set; } = string.Empty;
        public int JalaliYear { get; set; }
        public int JalaliMonth { get; set; }
        public int Count { get; set; }
    }

    public class RewardPopularityRow
    {
        public string Title { get; set; } = string.Empty;
        public int RequestCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PendingCount { get; set; }
    }
}
