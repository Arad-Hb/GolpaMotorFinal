using DomainModel.ViewModels.Reward;
using GolpaMotorFinal.Models.ViewModels;

namespace GolpaMotorFinal.Models.ViewModels.RewardManagement
{
    public class RewardRequestListPageViewModel
    {
        public List<RewardRequestListItem> Items { get; set; } = new();
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public int RecordCount { get; set; }
        public RewardRequestSearchModel Filter { get; set; } = new();
    }
}
