using GolpaMotorFinal.Models.ViewModels.CRUD;

namespace GolpaMotorFinal.Models.ViewModels.Account
{
    public class EligibleRewardsDialogViewModel
    {
        public string UserID { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? JobTitle { get; set; }
        public int TotalEarnedPoints { get; set; }
        public int TotalSettledPoints { get; set; }
        public int RemainedPoints { get; set; }
        public int AvailablePoints { get; set; }
        public int TotalRegisteredCards { get; set; }
        public bool IsEligibleForReward { get; set; }
        public bool HasReceivedReward { get; set; }
        public CrudGridViewModel EligibleRewardsGrid { get; set; } = new();
        public CrudGridViewModel RewardHistoryGrid { get; set; } = new();
    }
}
