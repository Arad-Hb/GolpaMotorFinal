using DataAccess.Services;
using DomainModel.ViewModels.Product;
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
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var products = await repo.GetAll();
            return View(products);
        }
    }
}
