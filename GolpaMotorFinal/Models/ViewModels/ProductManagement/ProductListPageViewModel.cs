using DomainModel.ViewModels.Product;
using GolpaMotorFinal.Models.ViewModels;

namespace GolpaMotorFinal.Models.ViewModels.ProductManagement
{
    public class ProductListPageViewModel
    {
        public List<ProductListItem> Items { get; set; } = new();
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public int RecordCount { get; set; }
        public ProductSearchModel Filter { get; set; } = new();
    }
}
