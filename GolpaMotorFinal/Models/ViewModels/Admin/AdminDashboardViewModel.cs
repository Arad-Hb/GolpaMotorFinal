using DomainModel.ViewModels.Product;
using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.Reward;
using GolpaMotorFinal.Models.ViewModels.Admin;

namespace GolpaMotorFinal.Models.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public ProductStatistics? Stats { get; set; }
        public IEnumerable<NamedCountItem> TopProducts { get; set; } = Enumerable.Empty<NamedCountItem>();
        public IEnumerable<NamedCountItem> TopRewards { get; set; } = Enumerable.Empty<NamedCountItem>();
        public AdminDashboardSummary Summary { get; set; } = new();
        public List<RewardRequestListItem> PendingRewardRequests { get; set; } = new();
        public List<DashboardWarrantyAlertItem> WarrantyAlerts { get; set; } = new();
        public TopRegistrarsPageViewModel TopRegistrars { get; set; } = new();
    }
}
