using DomainModel.ViewModels.Reward;

namespace GolpaMotorFinal.Models.ViewModels.Account
{
    public class EligibleRewardsDialogViewModel
    {
        public string UserID { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int TotalEarnedPoints { get; set; }
        public int TotalSettledPoints { get; set; }
        public int RemainedPoints { get; set; }
        public int TotalRegisteredCards { get; set; }
        public bool IsEligibleForReward { get; set; }
        public bool HasReceivedReward { get; set; }
        public List<UserEligibleRewardItem> Items { get; set; } = new();
        public List<RewardRequestListItem> RecentRequests { get; set; } = new();
    }
}
