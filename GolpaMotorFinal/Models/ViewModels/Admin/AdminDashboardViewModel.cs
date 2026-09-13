using DomainModel.ViewModels.Product;
using GolpaMotorFinal.Models.ViewModels.Admin;

namespace GolpaMotorFinal.Models.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public ProductStatistics? Stats { get; set; }
        public IEnumerable<NamedCountItem> TopProducts { get; set; } = Enumerable.Empty<NamedCountItem>();
        public IEnumerable<NamedCountItem> TopRewards { get; set; } = Enumerable.Empty<NamedCountItem>();
        public TopRegistrarsPageViewModel TopRegistrars { get; set; } = new();
    }
}
