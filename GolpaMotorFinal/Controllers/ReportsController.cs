using Application.Services;
using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.User;
using Framework.Common.Extensions;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Mappers;
using GolpaMotorFinal.Models.ViewModels.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly IReportService reports;
        private readonly ReportPageBuilder pageBuilder;

        public ReportsController(IReportService reports, ReportPageBuilder pageBuilder)
        {
            this.reports = reports;
            this.pageBuilder = pageBuilder;
        }

        [HttpGet("/reports")]
        public IActionResult Index(string? tab = null)
        {
            var tabs = ReportTabDefinitions.CreateTabs();
            var selectedKey = (tab ?? ReportRoutes.Users).ToLowerInvariant();
            var selectedTab = tabs.FirstOrDefault(item => item.Key == selectedKey)
                ?? tabs.First(item => item.Key == ReportRoutes.Users);

            return Redirect(ReportRoutes.TabPath(selectedTab.Key));
        }

        [HttpGet("/Reports/Index")]
        public IActionResult LegacyIndex() => Redirect(ReportRoutes.TabPath(ReportRoutes.Users));

        [HttpGet("/reports/users")]
        [HttpGet("/reports/productwarranty")]
        [HttpGet("/reports/rewards")]
        [HttpGet("/reports/userstransactiondetails")]
        [HttpGet("/reports/productwarrantytransactiondetails")]
        [HttpGet("/reports/rewardstransactiondetails")]
        public async Task<IActionResult> ReportTab()
        {
            var tabs = ReportTabDefinitions.CreateTabs();
            var selectedKey = (Request.Path.Value ?? "").TrimEnd('/').Split('/').Last().ToLowerInvariant();
            var selectedTab = tabs.FirstOrDefault(item => item.Key == selectedKey);

            if (selectedTab is null)
            {
                return NotFound();
            }

            var page = await pageBuilder.BuildAsync(selectedTab, Request.Query);

            var model = new ReportsIndexViewModel
            {
                OpenTab = selectedTab.Key,
                Tabs = tabs,
                Page = page,
                CurrentJalaliYear = DateTime.UtcNow.ToIranTime().GetPersianYear()
            };

            return View("Index", model);
        }

        [HttpGet("/Reports/UserDetails/{userId}")]
        public IActionResult UserDetails(string userId) =>
            Redirect(ReportRoutes.UserTransactions(userId));

        [HttpGet("/Reports/ProductDetails/{productId:long}")]
        public IActionResult ProductDetails(long productId) =>
            Redirect(ReportRoutes.ProductCardDetails(productId));

        [HttpGet("/Reports/CardDetails/{warrantyCardId:long}")]
        public IActionResult CardDetails(long warrantyCardId) =>
            Redirect(ReportRoutes.ProductCardDetails(warrantyCardId: warrantyCardId));

        [HttpGet("/Reports/RewardDetails/{rewardRequestId:int}")]
        public IActionResult RewardDetails(int rewardRequestId) =>
            Redirect(ReportRoutes.RewardRequest(rewardRequestId));

        [HttpGet("/Reports/RewardCatalogDetails/{rewardCatalogId:int}")]
        public IActionResult RewardCatalogDetails(int rewardCatalogId) =>
            Redirect(ReportRoutes.RewardCatalog(rewardCatalogId));

        [HttpGet("/Reports/Activities")]
        public IActionResult ActivitiesRedirect(ReportActivitySearchModel search) =>
            Redirect(ReportRoutes.TabUrl(ReportRoutes.UserTransactionDetails, ActivityQuery(search)));

        [HttpGet("/Reports/ProductCards")]
        public IActionResult ProductCardsRedirect(ReportActivitySearchModel search) =>
            Redirect(ReportRoutes.TabUrl(ReportRoutes.ProductWarrantyTransactionDetails, ActivityQuery(search)));

        [HttpGet("/Reports/Warranty")]
        public IActionResult WarrantyRedirect(long? productId, string? fromJalali, string? toJalali) =>
            Redirect(ReportRoutes.TabUrl(ReportRoutes.ProductWarranty, new
            {
                ProductID = productId,
                FromJalali = fromJalali,
                ToJalali = toJalali
            }));

        [HttpGet("/Reports/Products")]
        public IActionResult ProductsRedirect() =>
            Redirect(ReportRoutes.TabPath(ReportRoutes.ProductWarranty));

        [HttpGet("/reports/grid/users")]
        public async Task<IActionResult> UsersGrid(UserSearchModel search)
        {
            if (!ModelState.IsValid)
            {
                return ReportGridResult.InvalidFilter(this);
            }

            var result = await reports.SearchUsers(search);
            var pagerUrl = FilterUrl.Combine(ReportRoutes.GridPath(ReportRoutes.Users), new
            {
                search.SearchTerm,
                search.CustomerTypeID,
                search.ProvinceID,
                search.CityID,
                search.PointsFrom,
                search.PointsTo,
                search.IsEligibleForReward,
                search.HasReceivedReward,
                search.CardFromJalali,
                search.CardToJalali
            });

            var grid = AdminListGrids.BuildUserReportGrid(
                (result.Data?.Items ?? new List<UserListItem>())
                    .Select(UserViewMapper.ToReport)
                    .ToList());

            return ReportGridResult.FromSearch(this, result, grid, "UserReportGrid", pagerUrl);
        }

        [HttpGet("/reports/grid/productwarranty")]
        public async Task<IActionResult> ProductWarrantyGrid(ProductWarrantyReportSearchModel search)
        {
            if (!ModelState.IsValid)
            {
                return ReportGridResult.InvalidFilter(this);
            }

            var result = await reports.SearchProductWarranty(search);
            var pagerUrl = FilterUrl.Combine(ReportRoutes.GridPath(ReportRoutes.ProductWarranty), new
            {
                search.ProductID,
                search.SearchTerm,
                search.IsRegistered,
                search.CountFrom,
                search.CountTo,
                search.PointsFrom,
                search.PointsTo,
                search.FromJalali,
                search.ToJalali
            });

            var grid = AdminListGrids.BuildProductWarrantyReportGrid(result.Data?.Items ?? new List<ProductWarrantyReportRow>());
            return ReportGridResult.FromSearch(this, result, grid, "ProductWarrantyReportGrid", pagerUrl);
        }

        [HttpGet("/reports/grid/rewards")]
        public async Task<IActionResult> RewardsGrid(string? fromJalali, string? toJalali, int pageIndex = 0)
        {
            var result = await reports.SearchRewards(fromJalali, toJalali, pageIndex);
            var pagerUrl = FilterUrl.Combine(ReportRoutes.GridPath(ReportRoutes.Rewards), new { fromJalali, toJalali });
            var grid = AdminListGrids.BuildRewardReportGrid(result.Data?.Items ?? new List<RewardPopularityRow>());
            return ReportGridResult.FromSearch(this, result, grid, "RewardReportGrid", pagerUrl);
        }

        [HttpGet("/reports/grid/userstransactiondetails")]
        public async Task<IActionResult> UsersTransactionDetailsGrid(ReportActivitySearchModel search)
        {
            if (!ModelState.IsValid)
            {
                return ReportGridResult.InvalidFilter(this);
            }

            var result = await reports.SearchUserTransactions(search);
            var pagerUrl = ReportRoutes.ActivitySearchPagerUrl(ReportRoutes.UserTransactionDetails, search);
            var grid = AdminListGrids.BuildReportActivityGrid(result.Data?.Items ?? new List<ReportActivityItem>());
            return ReportGridResult.FromSearch(this, result, grid, "ReportActivityGrid", pagerUrl);
        }

        [HttpGet("/reports/grid/productwarrantytransactiondetails")]
        public async Task<IActionResult> ProductWarrantyTransactionDetailsGrid(ReportActivitySearchModel search)
        {
            if (!ModelState.IsValid)
            {
                return ReportGridResult.InvalidFilter(this);
            }

            var result = await reports.SearchProductWarrantyTransactions(search);
            var pagerUrl = ReportRoutes.ActivitySearchPagerUrl(ReportRoutes.ProductWarrantyTransactionDetails, search);
            var grid = AdminListGrids.BuildProductCardDetailsGrid(result.Data?.Items ?? new List<ProductCardDetailItem>());
            return ReportGridResult.FromSearch(this, result, grid, "ProductCardDetailGrid", pagerUrl);
        }

        [HttpGet("/reports/grid/rewardstransactiondetails")]
        public async Task<IActionResult> RewardsTransactionDetailsGrid(ReportActivitySearchModel search)
        {
            if (!ModelState.IsValid)
            {
                return ReportGridResult.InvalidFilter(this);
            }

            var result = await reports.SearchRewardTransactions(search);
            var pagerUrl = ReportRoutes.ActivitySearchPagerUrl(ReportRoutes.RewardTransactionDetails, search);
            var grid = AdminListGrids.BuildReportActivityGrid(result.Data?.Items ?? new List<ReportActivityItem>());
            return ReportGridResult.FromSearch(this, result, grid, "ReportActivityGrid", pagerUrl);
        }

        private static object ActivityQuery(ReportActivitySearchModel search) => new
        {
            search.SearchTerm,
            search.ProductID,
            search.UserID,
            search.WarrantyCardID,
            search.RewardRequestID,
            search.RewardCatalogID,
            search.ActivityType,
            search.RewardStatus,
            search.IsRegistered,
            search.PointsFrom,
            search.PointsTo,
            search.FromJalali,
            search.ToJalali
        };
    }
}
