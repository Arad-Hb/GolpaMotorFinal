using DomainModel.ViewModels.Reward;

namespace GolpaMotorFinal.Models.ViewModels.RewardManagement
{
    public class RewardCatalogListPageViewModel
    {
        public List<RewardCatalogListItem> Items { get; set; } = new();
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public int RecordCount { get; set; }
        public string? Title { get; set; }
    }
}
