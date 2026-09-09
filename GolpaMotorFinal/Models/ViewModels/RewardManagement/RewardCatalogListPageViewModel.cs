using DomainModel.ViewModels.Reward;
using GolpaMotorFinal.Models.ViewModels;

namespace GolpaMotorFinal.Models.ViewModels.RewardManagement
{
    public class RewardCatalogListPageViewModel
    {
        public List<RewardCatalogListItem> Items { get; set; } = new();
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public int RecordCount { get; set; }
        public RewardCatalogSearchModel Filter { get; set; } = new();
    }
}
