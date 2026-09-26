using Application.Services;
using DataAccess.Services;
using DomainModel.ViewModels.User;
using Framework.Common.Extensions;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Mappers;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly IUserService users;
        private readonly IProductService products;
        private readonly IReportRepository reports;
        private readonly LookupLists lookups;

        public ReportsController(
            IUserService users,
            IProductService products,
            IReportRepository reports,
            LookupLists lookups)
        {
            this.users = users;
            this.products = products;
            this.reports = reports;
            this.lookups = lookups;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.CustomerTypes = await lookups.CustomerTypes();
            ViewBag.Provinces = await lookups.Provinces();

            return View(new ReportsIndexViewModel
            {
                Products = await products.GetAll()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Users(UserSearchModel sm)
        {
            sm.CardFrom = sm.CardFromJalali.ToGregorianDate();
            sm.CardTo = sm.CardToJalali.ToGregorianDate();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var result = await users.Search(sm);
            var page = result.sm ?? sm;
            var reportUsers = (result.userList ?? new List<UserListItem>()).Select(UserViewMapper.ToReport).ToList();
            var grid = AdminListGrids.BuildUserReportGrid(reportUsers);
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
            var from = fromJalali.ToGregorianDate();
            var to = toJalali.ToGregorianDate();
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
                    fromJalali.ToGregorianDate(),
                    toJalali.ToGregorianDate()),
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
