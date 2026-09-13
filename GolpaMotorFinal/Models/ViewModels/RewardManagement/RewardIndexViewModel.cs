using DomainModel.Models;
using DomainModel.ViewModels.Reward;

namespace GolpaMotorFinal.Models.ViewModels.RewardManagement
{
    public class RewardIndexViewModel
    {
        public IEnumerable<RewardCatalogListItem> Catalogs { get; set; } = Enumerable.Empty<RewardCatalogListItem>();
        public IEnumerable<RewardDeliveryStatus> Statuses { get; set; } = Enumerable.Empty<RewardDeliveryStatus>();
    }
}
