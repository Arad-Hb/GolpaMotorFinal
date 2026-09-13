using DataAccess.Services;
using DomainModel.ViewModels.User;
using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly IUserService users;
        private readonly IUserRepository userRepo;
        private readonly IProductRepository products;
        private readonly IReportRepository reports;

        public ReportsController(
            IUserService users,
            IUserRepository userRepo,
            IProductRepository products,
            IReportRepository reports)
        {
            this.users = users;
            this.userRepo = userRepo;
            this.products = products;
            this.reports = reports;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.CustomerTypes = await userRepo.GetCustomerTypes();
            ViewBag.Provinces = await userRepo.GetProvinces();
            return View(new GolpaMotorFinal.Models.ViewModels.Reports.ReportsIndexViewModel
            {
                Products = await products.GetAll()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Users(UserSearchModel sm)
        {
            sm.CardFrom = PersianDate.ParseOrNull(sm.CardFromJalali);
            sm.CardTo = PersianDate.ParseOrNull(sm.CardToJalali);
            var page = await users.GetUserReportPage(sm);
            var grid = AdminListGrids.BuildUserReportGrid(page.Users);
            CrudGridPager.Attach(
                grid,
                "UserReportGrid",
                page.PageIndex,
                page.PageCount,
                page.RecordCount,
                FilterUrl.Combine("/Reports/Users", new
                {
                    sm.SearchTerm,
                    sm.CustomerTypeID,
                    sm.ProvinceID,
                    sm.CityID,
                    sm.PointsFrom,
                    sm.PointsTo,
                    sm.IsEligibleForReward,
                    sm.HasReceivedReward,
                    sm.CardFromJalali,
                    sm.CardToJalali
                }));
            return ViewComponent("CrudGrid", new { model = grid });
        }

        [HttpGet]
        public async Task<IActionResult> Warranty(long? productId, string? fromJalali, string? toJalali, int pageIndex = 0)
        {
            var from = PersianDate.ParseOrNull(fromJalali);
            var to = PersianDate.ParseOrNull(toJalali);
            var page = CrudGridPager.Slice(await reports.GetWarrantyByProduct(productId, from, to), pageIndex);
            var grid = AdminListGrids.BuildWarrantyReportGrid(page.Items);
            CrudGridPager.Attach(
                grid,
                "WarrantyReportGrid",
                page.PageIndex,
                page.PageCount,
                page.RecordCount,
                FilterUrl.Combine("/Reports/Warranty", new { productId, fromJalali, toJalali }));
            return ViewComponent("CrudGrid", new { model = grid });
        }

        [HttpGet]
        public async Task<IActionResult> Products(int? year, int? month, int pageIndex = 0)
        {
            var page = CrudGridPager.Slice(await reports.GetProductPopularity(jalaliYear: year, jalaliMonth: month), pageIndex);
            var grid = AdminListGrids.BuildProductReportGrid(page.Items);
            CrudGridPager.Attach(
                grid,
                "ProductReportGrid",
                page.PageIndex,
                page.PageCount,
                page.RecordCount,
                FilterUrl.Combine("/Reports/Products", new { year, month }));
            return ViewComponent("CrudGrid", new { model = grid });
        }

        [HttpGet]
        public async Task<IActionResult> Rewards(string? fromJalali, string? toJalali, int pageIndex = 0)
        {
            var page = CrudGridPager.Slice(
                await reports.GetRewardPopularity(
                    PersianDate.ParseOrNull(fromJalali),
                    PersianDate.ParseOrNull(toJalali)),
                pageIndex);
            var grid = AdminListGrids.BuildRewardReportGrid(page.Items);
            CrudGridPager.Attach(
                grid,
                "RewardReportGrid",
                page.PageIndex,
                page.PageCount,
                page.RecordCount,
                FilterUrl.Combine("/Reports/Rewards", new { fromJalali, toJalali }));
            return ViewComponent("CrudGrid", new { model = grid });
        }
    }
}
