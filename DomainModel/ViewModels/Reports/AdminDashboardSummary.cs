namespace DomainModel.ViewModels.Reports
{
    public class AdminDashboardSummary
    {
        public int TotalEarnedPoints { get; set; }
        public int TotalSettledPoints { get; set; }
        public int TotalRemainedPoints { get; set; }
        public int TotalRewardRequests { get; set; }
        public int SettledRewardRequests { get; set; }
        public int PendingRewardRequests { get; set; }
    }
}
