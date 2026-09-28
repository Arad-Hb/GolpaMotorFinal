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

            var model = ProductViewMapper.ToAddEditModel(vm);
            model.ImageUrl = "/images/pics/noimage.jpg";

            var uploaded = await TryUpload(vm.ImageFile);
            if (uploaded is { Success: false })
                return Json(new OperationResult("AddProduct").ToFailed(uploaded.Message));
            if (uploaded is { Success: true })
                model.ImageUrl = uploaded.FileUrl;

            var result = await service.AddProduct(model);
            if (!result.Success && uploaded is { Success: true })
                RemoveImage(uploaded.FileUrl);

            return Json(result);
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
            model.ImageUrl =string.IsNullOrWhiteSpace(current.ImageUrl)
                ? "/images/pics/noimage.jpg"
                : current.ImageUrl;

            var uploaded = await TryUpload(vm.ImageFile);
            if (uploaded is { Success: false })
                return Json(new OperationResult("UpdateProduct").ToFailed(uploaded.Message));
            if (uploaded is { Success: true })
            {
                RemoveImage(current.ImageUrl);
                model.ImageUrl = uploaded.FileUrl;
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

        private async Task<FileUploadResult?> TryUpload(IFormFile? image)
        {
            if (image == null)
                return null;
            return await fileManager.UploadAsync(
                image, 5, new[] { "jpg", "jpeg", "png" },
                "images/imageProducts/uploads", "images/imageProducts/thumbnails");
        }
        private void RemoveImage(string? url)
        {
            if (string.IsNullOrWhiteSpace(url) || url == "/images/pics/noimage.jpg")
                return;
            fileManager.Remove(url);
            fileManager.Remove(url.Replace("/images/imageProducts/uploads/", "/images/imageProducts/thumbnails/"));
        }
    }
}
