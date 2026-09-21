using Application.Services;
using DomainModel.ViewModels.Product;
using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Mappers;
using GolpaMotorFinal.Models;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.ProductManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductManagementController : Controller
    {
        private static readonly string[] ImageExtensions = { "jpg", "jpeg", "png" };
        private const string ProductUploadFolder = "images/imageProducts/uploads";
        private const string ProductThumbFolder = "images/imageProducts/thumbnails";

        private readonly IProductService service;
        private readonly IFileManager fileManager;

        public ProductManagementController(IProductService service, IFileManager fileManager)
        {
            this.service = service;
            this.fileManager = fileManager;
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

            if (vm.ImageFile == null)
                return Json(new { success = false, message = "تصویر محصول الزامی است" });

            var upload = await fileManager.UploadAsync(
                vm.ImageFile, 5, ImageExtensions, ProductUploadFolder, ProductThumbFolder);
            if (!upload.Success)
                return Json(new { success = false, message = upload.Message });

            var model = ProductViewMapper.ToAddEditModel(vm);
            model.ImageUrl = upload.FileUrl;
            var result = await service.AddProduct(model);
            if (!result.Success)
                fileManager.Remove(upload.FileUrl);

            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long productID)
        {
            var prod = await service.Get(productID);
            if (prod == null)
                return NotFound();

            return PartialView("_Edit", ProductViewMapper.ToAddEditViewModel(prod));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductAddEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات معتبر نیست" });

            var current = await service.Get(vm.ProductID);
            if (current == null)
                return Json(new { success = false, message = "محصول یافت نشد" });

            var model = ProductViewMapper.ToAddEditModel(vm);
            model.ImageUrl = current.ImageUrl;

            if (vm.ImageFile != null)
            {
                var upload = await fileManager.UploadAsync(
                    vm.ImageFile, 5, ImageExtensions, ProductUploadFolder, ProductThumbFolder);
                if (!upload.Success)
                    return Json(new OperationResult("UpdateProduct").ToFailed(upload.Message));

                if (!string.IsNullOrWhiteSpace(current.ImageUrl))
                    fileManager.Remove(current.ImageUrl);

                model.ImageUrl = upload.FileUrl;
            }

            return Json(await service.UpdateProduct(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(long productID)
        {
            var product = await service.Get(productID);
            var result = await service.DeleteProduct(productID);
            if (result.Success && !string.IsNullOrEmpty(product?.ImageUrl))
                fileManager.Remove(product.ImageUrl);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(long productID)
        {
            var prod = await service.GetDetails(productID);
            if (prod == null)
                return NotFound();
            return PartialView("_Details", prod);
        }
    }
}
