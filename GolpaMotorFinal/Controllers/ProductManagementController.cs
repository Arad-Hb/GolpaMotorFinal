using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Product;
using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Models.ViewModels.ProductManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GolpaMotorFinal.Controllers
{
    public class ProductManagementController : Controller
    {
        private readonly IProductRepository repo;        
        private readonly IProductService service;
        public ProductManagementController(IProductRepository repo, IProductService service)
        {
            this.repo = repo;           
            this.service = service;
        }
        public IActionResult Index()
        {
            return View();
        }              

        [HttpGet]
        public IActionResult ProductList()
        {
            return ViewComponent("ProductList");
        }

        [HttpGet]
        public async Task<JsonResult> Get(long ProductID)
        {
            var product = await repo.Get(ProductID);
            return Json(product);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await repo.GetAll();
            return Json(products);
        }
        
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(ProductAddEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات معتبر نیست" });

            var model = new ProductAddEditModel
            {
                ProductName = vm.ProductName,
                Description = vm.Description,
                ProductPoint = vm.ProductPoint,
                IsAvailable = vm.IsAvailable
            };

            var result = await service.AddProduct(model, vm.ImageFile);

            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long productID)
        {
            var prod = await repo.Get(productID);

            if (prod == null)
                return NotFound();

            var vm = new ProductAddEditViewModel
            {
                ProductID = prod.ProductID,
                ProductName = prod.ProductName,
                Description = prod.Description,
                ProductPoint = prod.ProductPoint,
                IsAvailable = prod.IsAvailable,
                ExistingImageUrl = prod.ImageUrl
            };

            return PartialView("_Edit", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductAddEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات معتبر نیست" });

            // تبدیل ViewModel → Domain Model
            var model = new ProductAddEditModel
            {
                ProductID = vm.ProductID,
                ProductName = vm.ProductName,
                Description = vm.Description,
                ProductPoint = vm.ProductPoint,
                IsAvailable = vm.IsAvailable,
                ImageUrl = vm.ExistingImageUrl // عکس قبلی
            };

            // ارسال به Service همراه فایل جدید
            var result = await service.UpdateProduct(model, vm.ImageFile);

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(long productID)
        {
            var result = await repo.Delete(productID);

            return Json(result);
        }
        
        [HttpGet]
        public async Task<IActionResult> Details(long productID)
        {
            var prod = await repo.GetDetails(productID);

            if (prod == null)
                return NotFound();

            return PartialView("_Details", prod);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RemovePicture(long productID)
        {
            var result = await service.RemovePicture(productID);
            return Json(result);
        }
    }
}
