using DomainModel.ViewModels.Product;

namespace GolpaMotorFinal.Models.ViewModels.ProductManagement
{
    public class ProductListPageViewModel
    {
        public List<ProductListItem> Items { get; set; } = new();
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public int RecordCount { get; set; }
        public string? ProductName { get; set; }
    }
}
