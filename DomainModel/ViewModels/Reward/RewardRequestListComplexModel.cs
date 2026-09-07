namespace DomainModel.ViewModels.Reward
{
    public class RewardRequestListComplexModel
    {
        public List<RewardRequestListItem> RequestList { get; set; } = new();
        public RewardRequestSearchModel sm { get; set; } = new();
    }
}
