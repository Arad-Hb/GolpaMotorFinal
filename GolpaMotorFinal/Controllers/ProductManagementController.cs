using DomainModel.ViewModels.Product;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.ProductManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductManagementController : Controller
    {
        private readonly IProductService service;

        public ProductManagementController(IProductService service)
        {
            this.service = service;
        }

        public async Task<IActionResult> Index()
        {
            return View(await service.GetStatistics());
        }

        [HttpGet]
        public async Task<IActionResult> List(ProductSearchModel sm)
        {
            sm ??= new ProductSearchModel();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var result = await service.Search(sm);
            var filter = result.sm ?? sm;
            var grid = AdminListGrids.BuildProductGrid(result.productList ?? new List<ProductListItem>());
            CrudGridPager.Attach(
                grid,
                "ProductGrid",
                filter.PageIndex,
                filter.PageCount,
                filter.RecordCount,
                FilterUrl.Combine("/ProductManagement/List", new
                {
                    filter.ProductName,
                    filter.IsAvailable,
                    filter.PointsFrom,
                    filter.PointsTo,
                    filter.RegisteredFrom,
                    filter.RegisteredTo,
                    filter.RemainingFrom,
                    filter.RemainingTo
                }));
            return ViewComponent("CrudGrid", new { model = grid });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_Create", new ProductAddEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(ProductAddEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات معتبر نیست" });

            var result = await service.AddProduct(new ProductAddEditModel
            {
                ProductName = vm.ProductName,
                Description = vm.Description,
                ProductPoint = vm.ProductPoint,
                IsAvailable = vm.IsAvailable
            }, vm.ImageFile);

            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long productID)
        {
            var prod = await service.GetForEdit(productID);
            if (prod == null)
                return NotFound();

            return PartialView("_Edit", new ProductAddEditViewModel
            {
                ProductID = prod.ProductID,
                ProductName = prod.ProductName,
                Description = prod.Description,
                ProductPoint = prod.ProductPoint,
                IsAvailable = prod.IsAvailable,
                ExistingImageUrl = prod.ImageUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductAddEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات معتبر نیست" });

            var result = await service.UpdateProduct(new ProductAddEditModel
            {
                ProductID = vm.ProductID,
                ProductName = vm.ProductName,
                Description = vm.Description,
                ProductPoint = vm.ProductPoint,
                IsAvailable = vm.IsAvailable,
                ImageUrl = vm.ExistingImageUrl
            }, vm.ImageFile);

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(long productID)
            => Json(await service.DeleteProduct(productID));

        [HttpGet]
        public async Task<IActionResult> Details(long productID)
        {
            var prod = await service.GetDetails(productID);
            if (prod == null)
                return NotFound();
            return PartialView("_Details", prod);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<JsonResult> RemovePicture(long productID)
        //    => Json(await service.RemovePicture(productID));
    }
}
