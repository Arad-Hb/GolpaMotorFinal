using DataAccess.Services;
using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.User;
using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.CRUD;
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
            ViewBag.Products = await products.GetAll();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Users(UserSearchModel sm)
        {
            sm.CardFrom = PersianDate.ParseOrNull(sm.CardFromJalali);
            sm.CardTo = PersianDate.ParseOrNull(sm.CardToJalali);
            var page = await users.GetUserReportPage(sm);
            var grid = users.BuildUserReportGrid(page.Users);
            grid.GridId = "UserReportGrid";
            var pager = PaginationViewModel.For(
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
            ViewBag.Pager = pager;
            return PartialView("_UserReportGrid", grid);
        }

        [HttpGet]
        public async Task<IActionResult> Warranty(long? productId, string? fromJalali, string? toJalali)
        {
            var from = PersianDate.ParseOrNull(fromJalali);
            var to = PersianDate.ParseOrNull(toJalali);
            var rows = await reports.GetWarrantyByProduct(productId, from, to);
            return PartialView("_WarrantyReport", rows);
        }

        [HttpGet]
        public async Task<IActionResult> Products(int? year, int? month)
        {
            var rows = await reports.GetProductPopularity(jalaliYear: year, jalaliMonth: month);
            return PartialView("_ProductReport", rows);
        }

        [HttpGet]
        public async Task<IActionResult> Rewards(string? fromJalali, string? toJalali)
        {
            var rows = await reports.GetRewardPopularity(
                PersianDate.ParseOrNull(fromJalali),
                PersianDate.ParseOrNull(toJalali));
            return PartialView("_RewardReport", rows);
        }
    }
}
