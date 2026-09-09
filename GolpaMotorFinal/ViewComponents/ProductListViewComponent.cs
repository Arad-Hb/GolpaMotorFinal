using DataAccess.Services;
using DomainModel.ViewModels.Product;
using GolpaMotorFinal.Models.ViewModels;
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
        public async Task<IViewComponentResult> InvokeAsync(ProductSearchModel? sm = null)
        {
            sm ??= new ProductSearchModel();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var result = await repo.Search(sm);
            var vm = new ProductListPageViewModel
            {
                Items = result.productList,
                PageIndex = result.sm.PageIndex,
                PageCount = result.sm.PageCount,
                RecordCount = result.sm.RecordCount,
                Filter = result.sm ?? sm
            };
            return View(vm);
        }
    }
}
