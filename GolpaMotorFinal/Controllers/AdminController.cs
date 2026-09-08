using DomainModel.ViewModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DomainModel.Models;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Models.ViewModels.Admin;
using GolpaMotorFinal.Models.ViewModels;
using DataAccess.Services;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IProductRepository products;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IFileManager fileManager;

        public AdminController(
            IProductRepository products,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IFileManager fileManager)
        {
            this.products = products;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.fileManager = fileManager;
        }

        public async Task<IActionResult> Index(int pageIndex = 0)
        {
            var stats = await products.GetStatistics();
            var pageSize = PaginationViewModel.DefaultPageSize;
            var page = await products.GetTopRegistrarsPage(pageIndex, pageSize);
            ViewBag.Stats = stats;
            ViewBag.TopRegistrars = new TopRegistrarsPageViewModel
            {
                Items = page.Items,
                PageIndex = pageIndex,
                PageCount = pageSize <= 0 ? 1 : (int)Math.Ceiling(page.Total / (double)pageSize),
                RecordCount = page.Total
            };
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TopRegistrars(int pageIndex = 0)
        {
            var pageSize = PaginationViewModel.DefaultPageSize;
            var page = await products.GetTopRegistrarsPage(pageIndex, pageSize);
            var vm = new TopRegistrarsPageViewModel
            {
                Items = page.Items,
                PageIndex = pageIndex,
                PageCount = pageSize <= 0 ? 1 : (int)Math.Ceiling(page.Total / (double)pageSize),
                RecordCount = page.Total
            };
            return PartialView("_TopRegistrarsTable", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            return View(ToProfile(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(AdminProfileViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(ToProfile(user));

            user.FirstName = model.FirstName?.Trim();
            user.LastName = model.LastName?.Trim();

            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != user.Email)
            {
                user.Email = model.Email.Trim();
                user.UserName = model.Email.Trim();
            }

            if (model.ProfileImage != null)
            {
                var upload = await fileManager.UploadAsync(
                    model.ProfileImage,
                    5,
                    new[] { "jpg", "jpeg", "png" },
                    "images/imageUsers/uploads",
                    "images/imageUsers/thumbnails");

                if (!upload.Success)
                {
                    ModelState.AddModelError(string.Empty, upload.Message);
                    return View(ToProfile(user));
                }

                user.ProfileImageUrl = upload.FileUrl;
            }

            var update = await userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                foreach (var error in update.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(ToProfile(user));
            }

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    ModelState.AddModelError(nameof(model.CurrentPassword), "رمز فعلی را وارد کنید.");
                    return View(ToProfile(user));
                }

                var changed = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changed.Succeeded)
                {
                    foreach (var error in changed.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(ToProfile(user));
                }
            }

            await signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "پروفایل با موفقیت به‌روزرسانی شد.";
            return RedirectToAction(nameof(Profile));
        }

        private static AdminProfileViewModel ToProfile(ApplicationUser user)
        {
            return new AdminProfileViewModel
            {
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }
    }
}
