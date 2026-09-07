namespace DomainModel.ViewModels.Reward
{
    public class RewardCatalogListComplexModel
    {
        public List<RewardCatalogListItem> CatalogList { get; set; } = new();
        public RewardCatalogSearchModel sm { get; set; } = new();
    }
}
