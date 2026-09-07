using DomainModel.ViewModels.Reward;

namespace GolpaMotorFinal.Models.ViewModels.Account
{
    public class EligibleRewardsDialogViewModel
    {
        public int RemainedPoints { get; set; }
        public bool IsEligibleForReward { get; set; }
        public bool HasReceivedReward { get; set; }
        public List<UserEligibleRewardItem> Items { get; set; } = new();
        public List<RewardRequestListItem> RecentRequests { get; set; } = new();
    }
}
