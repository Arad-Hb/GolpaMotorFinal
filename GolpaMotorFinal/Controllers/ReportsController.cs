using Application.Services;
using DataAccess.Services;
using DomainModel.ViewModels.User;
using DomainModel.ViewModels.Reports;
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

        [HttpGet]
        public async Task<IActionResult> ProductWarranty(ProductWarrantyReportSearchModel sm)
        {
            sm.FromUtc = sm.FromJalali.JalaliStartOfDayUtc();
            sm.ToUtcExclusive = sm.ToJalali.JalaliEndExclusiveUtc();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var page = await reports.SearchProductWarranty(sm);
            var grid = AdminListGrids.BuildProductWarrantyReportGrid(page.Items);
            CrudGridPager.Attach(
                grid,
                "ProductWarrantyReportGrid",
                page.PageIndex,
                page.PageCount,
                page.RecordCount,
                FilterUrl.Combine("/Reports/ProductWarranty", new
                {
                    sm.ProductID,
                    sm.SearchTerm,
                    sm.IsRegistered,
                    sm.CountFrom,
                    sm.CountTo,
                    sm.PointsFrom,
                    sm.PointsTo,
                    sm.FromJalali,
                    sm.ToJalali
                }),
                page.PageSize);
            return ViewComponent("CrudGrid", new { model = grid });
        }

        [HttpGet]
        public async Task<IActionResult> Activities(ReportActivitySearchModel sm)
        {
            sm.FromUtc = sm.FromJalali.JalaliStartOfDayUtc();
            sm.ToUtcExclusive = sm.ToJalali.JalaliEndExclusiveUtc();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var page = await reports.SearchActivities(sm);
            var grid = AdminListGrids.BuildReportActivityGrid(page.Items);
            CrudGridPager.Attach(
                grid,
                "ReportActivityGrid",
                page.PageIndex,
                page.PageCount,
                page.RecordCount,
                ActivityUrl("/Reports/Activities", sm),
                page.PageSize);
            return ViewComponent("CrudGrid", new { model = grid });
        }

        [HttpGet]
        public async Task<IActionResult> ProductCards(ReportActivitySearchModel sm)
        {
            sm.FromUtc = sm.FromJalali.JalaliStartOfDayUtc();
            sm.ToUtcExclusive = sm.ToJalali.JalaliEndExclusiveUtc();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var page = await reports.SearchProductCards(sm);
            var grid = AdminListGrids.BuildProductCardDetailsGrid(page.Items);
            CrudGridPager.Attach(
                grid,
                "ProductCardDetailGrid",
                page.PageIndex,
                page.PageCount,
                page.RecordCount,
                ActivityUrl("/Reports/ProductCards", sm),
                page.PageSize);
            return ViewComponent("CrudGrid", new { model = grid });
        }

        [HttpGet]
        public async Task<IActionResult> ProductDetails(long id)
        {
            var product = await products.GetDetails(id);
            if (product == null)
                return NotFound();

            return View("ReportDetails", new ReportDetailsPageViewModel
            {
                Title = $"جزئیات کارت‌های محصول «{product.ProductName}»",
                GridId = "ProductCardDetailGrid",
                FilterPartial = "_ProductCardDetailFilterBar",
                Filter = new ReportActivityFilterBarViewModel
                {
                    Url = "/Reports/ProductCards",
                    Target = "#ProductCardDetailGrid",
                    ProductID = id,
                    Products = await products.GetAll()
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails(string id)
            => View("ReportDetails", await DetailPage(
                $"گردش کامل کاربر", userId: id));

        [HttpGet]
        public async Task<IActionResult> CardDetails(long id)
            => View("ReportDetails", await DetailPage(
                $"تاریخچه کارت گارانتی شماره {id}", warrantyCardId: id));

        [HttpGet]
        public async Task<IActionResult> RewardDetails(int id)
            => View("ReportDetails", await DetailPage(
                $"جزئیات درخواست پاداش شماره {id}", rewardRequestId: id));

        [HttpGet]
        public async Task<IActionResult> RewardCatalogDetails(int id)
            => View("ReportDetails", await DetailPage(
                "جزئیات درخواست‌های پاداش", rewardCatalogId: id));

        private async Task<ReportDetailsPageViewModel> DetailPage(
            string title,
            string? userId = null,
            long? warrantyCardId = null,
            int? rewardRequestId = null,
            int? rewardCatalogId = null)
        {
            return new ReportDetailsPageViewModel
            {
                Title = title,
                Filter = new ReportActivityFilterBarViewModel
                {
                    Products = await products.GetAll(),
                    UserID = userId,
                    WarrantyCardID = warrantyCardId,
                    RewardRequestID = rewardRequestId,
                    RewardCatalogID = rewardCatalogId
                }
            };
        }

        private static string ActivityUrl(string path, ReportActivitySearchModel sm)
            => FilterUrl.Combine(path, new
            {
                sm.SearchTerm,
                sm.ProductID,
                sm.UserID,
                sm.WarrantyCardID,
                sm.RewardRequestID,
                sm.RewardCatalogID,
                sm.ActivityType,
                sm.RewardStatus,
                sm.PointsFrom,
                sm.PointsTo,
                sm.FromJalali,
                sm.ToJalali
            });
    }
}
