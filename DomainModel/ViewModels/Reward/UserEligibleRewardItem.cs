namespace DomainModel.ViewModels.Reward
{
    public class UserEligibleRewardItem
    {
        public int RewardCatalogID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int RequiredPoints { get; set; }
        public bool IsCashReward { get; set; }
        public decimal? CashValue { get; set; }
        public bool HasPendingRequest { get; set; }
    }
}
