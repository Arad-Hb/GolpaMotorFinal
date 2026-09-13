using DomainModel.ViewModels.Product;

namespace GolpaMotorFinal.Models.ViewModels.Reports
{
    public class ReportsIndexViewModel
    {
        public IEnumerable<ProductListItem> Products { get; set; } = Enumerable.Empty<ProductListItem>();
    }
}
