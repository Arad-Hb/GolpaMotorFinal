using DomainModel.ViewModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DomainModel.Models;
using Application.Services;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Models.ViewModels.Admin;
using GolpaMotorFinal.Models.ViewModels;
using DataAccess.Services;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IProductService products;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IFileManager fileManager;

        private readonly IReportRepository reports;

        public AdminController(
            IProductService products,
            IReportRepository reports,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IFileManager fileManager)
        {
            this.products = products;
            this.reports = reports;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.fileManager = fileManager;
        }

        public async Task<IActionResult> Index(int pageIndex = 0)
        {
            var stats = await products.GetStatistics();
            var pageSize = PaginationViewModel.DefaultPageSize;
            var page = await reports.GetDashboardRegistrars(pageIndex, pageSize);
            return View(new AdminDashboardViewModel
            {
                Stats = stats,
                Summary = await reports.GetAdminDashboardSummary(),
                TopProducts = await reports.GetTopProducts(5),
                TopRewards = await reports.GetTopRewards(5),
                PendingRewardRequests = await reports.GetRecentPendingRewardRequests(8),
                WarrantyAlerts = await reports.GetWarrantyExpiryAlerts(10, 30),
                TopRegistrars = new TopRegistrarsPageViewModel
                {
                    Items = page.Items,
                    PageIndex = page.PageIndex,
                    PageCount = page.PageCount,
                    RecordCount = page.RecordCount
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> TopRegistrars(int pageIndex = 0)
        {
            var pageSize = PaginationViewModel.DefaultPageSize;
            var page = await reports.GetDashboardRegistrars(pageIndex, pageSize);
            var vm = new TopRegistrarsPageViewModel
            {
                Items = page.Items,
                PageIndex = page.PageIndex,
                PageCount = page.PageCount,
                RecordCount = page.RecordCount
            };
            return PartialView("_TopRegistrarsTable", vm);
        }

        [HttpGet]
        public async Task<IActionResult> PendingRewardRequests()
        {
            var grid = AdminListGrids.BuildRewardRequestGrid(
                await reports.GetRecentPendingRewardRequests(8));
            grid.GridId = "DashboardPendingRequestGrid";
            return ViewComponent("CrudGrid", new { model = grid });
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
