using Application.Services;
using DataAccess.Services;
using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.User;
using Framework.Common;
using Framework.Common.Extensions;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Mappers;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.CRUD;
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
        private readonly IReportService reports;
        private readonly LookupLists lookups;

        public ReportsController(IUserService users,IProductService products,IReportService reports,LookupLists lookups)
        {
            this.users = users;
            this.products = products;
            this.reports = reports;
            this.lookups = lookups;
        }

        private static IReadOnlyList<ReportTabViewModel> CreateTabs()
        {
            return new List<ReportTabViewModel>
                {
                    new ReportTabViewModel{Key = "users",Title = "گزارش کاربران"},
                    new ReportTabViewModel{Key = "productwarranty",Title = "گزارش گارانتی محصولات"},
                    new ReportTabViewModel{Key = "rewards",Title = "گزارش پاداش"},
                    new ReportTabViewModel{Key = "userstransactiondetails",Title = "جزئیات گردش کاربران"},
                    new ReportTabViewModel{Key = "productwarrantytransactiondetails",Title = "جزئیات کارت‌های گارانتی"},
                    new ReportTabViewModel{Key = "rewardstransactiondetails",Title = "جزئیات گردش پاداش"}
                };
        }

        [HttpGet("/reports")]
        [HttpGet("/reports/index")]
        public IActionResult Index(string? tab = null)
        {
            var tabs = CreateTabs();

            var selectedKey = (tab ?? "users").ToLowerInvariant();

            var selectedTab = tabs.FirstOrDefault(item => item.Key == selectedKey);

            if (selectedTab is null)
            {
                selectedTab = tabs.First(item => item.Key == "users");
            }

            return Redirect("/reports/" + selectedTab.Key);
        }

        [HttpGet("/reports/users")]
        [HttpGet("/reports/productwarranty")]
        [HttpGet("/reports/rewards")]
        [HttpGet("/reports/userstransactiondetails")]
        [HttpGet("/reports/productwarrantytransactiondetails")]
        [HttpGet("/reports/rewardstransactiondetails")]
        public async Task<IActionResult> ReportTab()
        {
            var tabs = CreateTabs();

            var selectedKey = (Request.Path.Value ?? "").TrimEnd('/').Split('/').Last().ToLowerInvariant();

            var selectedTab = tabs.FirstOrDefault(item => item.Key == selectedKey);

            if (selectedTab is null)
            {
                return NotFound();
            }

            var page = await BuildReportPageAsync(selectedTab);

            var model = new ReportsIndexViewModel
            {
                OpenTab = selectedTab.Key,
                Tabs = tabs,
                Page = page,
                CurrentJalaliYear =
                    DateTime.UtcNow.ToIranTime().GetPersianYear()
            };

            return View("Index", model);
        }

        [HttpGet("/reports/grid/users")]
        public Task<IActionResult> UsersGrid(UserSearchModel search)
        {
            var url = FilterUrl.Combine("/reports/grid/users", new
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

            return RenderGrid(
                () => reports.SearchUsers(search),
                rows => AdminListGrids.BuildUserReportGrid(
                    rows.Select(UserViewMapper.ToReport).ToList()),
                "UserReportGrid",
                url);
        }

        [HttpGet("/reports/grid/productwarranty")]
        public Task<IActionResult> ProductWarrantyGrid(ProductWarrantyReportSearchModel search)
        {
            var url = FilterUrl.Combine(
                "/reports/grid/productwarranty",
                new
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

            return RenderGrid(
                () => reports.SearchProductWarranty(search),
                rows => AdminListGrids.BuildProductWarrantyReportGrid(rows),
                "ProductWarrantyReportGrid",url);
        }

        [HttpGet("/reports/grid/rewards")]
        public Task<IActionResult> RewardsGrid(string? fromJalali,string? toJalali,int pageIndex = 0)
        {
            var url = FilterUrl.Combine(
                "/reports/grid/rewards",
                new { fromJalali, toJalali });

            return RenderGrid(
                () => reports.SearchRewards(
                    fromJalali,
                    toJalali,
                    pageIndex),
                rows => AdminListGrids.BuildRewardReportGrid(rows),
                "RewardReportGrid",
                url);
        }

        [HttpGet("/reports/grid/userstransactiondetails")]
        public Task<IActionResult> UsersTransactionDetailsGrid(ReportActivitySearchModel search)
        {
            return RenderGrid(
                () => reports.SearchUserTransactions(search),
                rows => AdminListGrids.BuildReportActivityGrid(rows),
                "ReportActivityGrid",
                ActivityUrl(
                    "/reports/grid/userstransactiondetails",
                    search));
        }

        [HttpGet("/reports/grid/productwarrantytransactiondetails")]
        public Task<IActionResult> ProductWarrantyTransactionDetailsGrid(ReportActivitySearchModel search)
        {
            return RenderGrid(
                () => reports.SearchProductWarrantyTransactions(search),
                rows => AdminListGrids.BuildProductCardDetailsGrid(rows),
                "ProductCardDetailGrid",
                ActivityUrl(
                    "/reports/grid/productwarrantytransactiondetails",
                    search));
        }

        [HttpGet("/reports/grid/rewardstransactiondetails")]
        public Task<IActionResult> RewardsTransactionDetailsGrid(ReportActivitySearchModel search)
        {
            return RenderGrid(
                () => reports.SearchRewardTransactions(search),
                rows => AdminListGrids.BuildReportActivityGrid(rows),
                "ReportActivityGrid",
                ActivityUrl("/reports/grid/rewardstransactiondetails",search));
        }

        // Preserve endpoints used by existing individual-detail pages.
        [HttpGet]
        public Task<IActionResult> Activities(ReportActivitySearchModel search)
        {
            return RenderGrid(
                () => reports.SearchUserTransactions(search),
                rows => AdminListGrids.BuildReportActivityGrid(rows),
                "ReportActivityGrid",
                ActivityUrl("/Reports/Activities", search));
        }

        [HttpGet]
        public Task<IActionResult> ProductCards(ReportActivitySearchModel search)
        {
            return RenderGrid(
                () => reports.SearchProductWarrantyTransactions(search),
                rows => AdminListGrids.BuildProductCardDetailsGrid(rows),
                "ProductCardDetailGrid",
                ActivityUrl("/Reports/ProductCards", search));
        }

        [HttpGet]
        public Task<IActionResult> Warranty(long? productId,string? fromJalali,string? toJalali,int pageIndex = 0)
        {
            return RenderGrid(
                () => reports.SearchWarranty(
                    productId,
                    fromJalali,
                    toJalali,
                    pageIndex),
                rows => AdminListGrids.BuildWarrantyReportGrid(rows),
                "WarrantyReportGrid",
                FilterUrl.Combine(
                    "/Reports/Warranty",
                    new { productId, fromJalali, toJalali }));
        }

        [HttpGet]
        public Task<IActionResult> Products(int? year,int? month,int pageIndex = 0)
        {
            return RenderGrid(
                () => reports.SearchProducts(year, month, pageIndex),
                rows => AdminListGrids.BuildProductReportGrid(rows),
                "ProductReportGrid",
                FilterUrl.Combine(
                    "/Reports/Products",
                    new { year, month }));
        }

        // Validate binding before executing the application service.
        private async Task<IActionResult> RenderGrid<T>(
            Func<Task<ReportSearchResult<ReportPage<T>>>> search,
            Func<List<T>, CrudGridViewModel> buildGrid,
            string gridId,
            string url)
        {
            if (!ModelState.IsValid)
            {
                var operation = new OperationResult("ReportSearch");

                operation.ToFailed(
                    "مقادیر فیلتر نامعتبر است.",
                    "InvalidFilter");

                return SearchFailure(operation);
            }

            var result = await search();

            if (!result.Operation.Success)
            {
                return SearchFailure(result.Operation);
            }

            var page = result.Data!;
            var grid = buildGrid(page.Items);

            CrudGridPager.Attach(
                grid,
                gridId,
                page.PageIndex,
                page.PageCount,
                page.RecordCount,
                url,
                page.PageSize);

            return ViewComponent("CrudGrid", new { model = grid });
        }

        private IActionResult SearchFailure(OperationResult operation)
        {
            Response.StatusCode =
                operation.ErrorCode == "InvalidFilter" ? 400 : 503;

            return PartialView("_SearchFailure", operation);
        }

        private static string ActivityUrl(string path,ReportActivitySearchModel search)
        {
            return FilterUrl.Combine(path, new
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
            });
        }

        private async Task<CrudIndexViewModel> BuildReportPageAsync(ReportTabViewModel selectedTab)
        {
            switch (selectedTab.Key)
            {
                case "users":
                    {
                        var customerTypes = await lookups.CustomerTypes();
                        var provinces = await lookups.Provinces();

                        return new CrudIndexViewModel
                        {
                            GridId = "UserReportGrid",
                            ListHeading = selectedTab.Title,

                            FilterPartial =
                                "~/Views/UserManagement/_UserFilterBar.cshtml",

                            FilterModel = new UserFilterBarViewModel
                            {
                                Url = "/reports/grid/users",
                                Target = "#UserReportGrid",
                                CustomerTypes = customerTypes,
                                Provinces = provinces
                            }
                        };
                    }

                case "productwarranty":
                    {
                        var productList = await products.GetAll();

                        return new CrudIndexViewModel
                        {
                            GridId = "ProductWarrantyReportGrid",
                            ListHeading = selectedTab.Title,
                            FilterPartial = "_ProductWarrantyReportFilterBar",

                            FilterModel =
                                new ProductWarrantyReportFilterBarViewModel
                                {
                                    Products = productList
                                }
                        };
                    }

                case "rewards":
                    {
                        return new CrudIndexViewModel
                        {
                            GridId = "RewardReportGrid",
                            ListHeading = selectedTab.Title,
                            FilterPartial = "_RewardReportFilterBar"
                        };
                    }

                case "userstransactiondetails":
                case "productwarrantytransactiondetails":
                case "rewardstransactiondetails":
                    {
                        var productList = await products.GetAll();

                        var cardDetails =
                            selectedTab.Key == "productwarrantytransactiondetails";

                        var rewardsOnly =
                            selectedTab.Key == "rewardstransactiondetails";

                        var gridId = cardDetails
                            ? "ProductCardDetailGrid"
                            : "ReportActivityGrid";

                        return new CrudIndexViewModel
                        {
                            GridId = gridId,
                            ListHeading = selectedTab.Title,
                            FilterPartial = "_TransactionFilterBar",

                            FilterModel = new ReportActivityFilterBarViewModel
                            {
                                Url = "/reports/grid/" + selectedTab.Key,
                                Target = "#" + gridId,
                                Products = productList,
                                CardDetails = cardDetails,
                                RewardsOnly = rewardsOnly
                            }
                        };
                    }

                default:
                    {
                        throw new InvalidOperationException(
                            "No report page configuration exists for tab: "
                            + selectedTab.Key);
                    }
            }
        }

    }
}