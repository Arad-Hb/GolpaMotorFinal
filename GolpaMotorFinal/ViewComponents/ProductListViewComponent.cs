using DataAccess.Services;
using DomainModel.ViewModels.Product;
using GolpaMotorFinal.Models.ViewModels.ProductManagement;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    [ViewComponent(Name = "ProductList")]
    public class ProductListViewComponent : ViewComponent
    {
        private readonly IProductRepository repo;
        public ProductListViewComponent(IProductRepository repo)
        {
            this.repo = repo;
        }
        public async Task<IViewComponentResult> InvokeAsync(string? productName = null, int pageIndex = 0)
        {
            var search = new ProductSearchModel
            {
                ProductName = productName,
                PageIndex = pageIndex,
                PageSize = 10
            };
            var result = await repo.Search(search);
            var vm = new ProductListPageViewModel
            {
                Items = result.productList,
                PageIndex = result.sm.PageIndex,
                PageCount = result.sm.PageCount,
                RecordCount = result.sm.RecordCount,
                ProductName = productName
            };
            return View(vm);
        }
    }
}
