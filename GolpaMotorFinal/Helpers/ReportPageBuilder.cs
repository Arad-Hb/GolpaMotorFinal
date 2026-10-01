using Application.Services;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.CRUD;
using GolpaMotorFinal.Models.ViewModels.Reports;
using Microsoft.AspNetCore.Http;

namespace GolpaMotorFinal.Helpers
{
    public class ReportPageBuilder
    {
        private readonly LookupLists lookups;
        private readonly IProductService products;

        public ReportPageBuilder(LookupLists lookups, IProductService products)
        {
            this.lookups = lookups;
            this.products = products;
        }

        public async Task<CrudIndexViewModel> BuildAsync(
            ReportTabViewModel selectedTab,
            IQueryCollection query)
        {
            switch (selectedTab.Key)
            {
                case ReportRoutes.Users:
                {
                    var customerTypes = await lookups.CustomerTypes();
                    var provinces = await lookups.Provinces();

                    return new CrudIndexViewModel
                    {
                        GridId = "UserReportGrid",
                        ListHeading = selectedTab.Title,
                        FilterPartial = "~/Views/UserManagement/_UserFilterBar.cshtml",
                        FilterModel = new UserFilterBarViewModel
                        {
                            Url = ReportRoutes.GridPath(ReportRoutes.Users),
                            Target = "#UserReportGrid",
                            CustomerTypes = customerTypes,
                            Provinces = provinces
                        }
                    };
                }

                case ReportRoutes.ProductWarranty:
                {
                    var productList = await products.GetAll();

                    return new CrudIndexViewModel
                    {
                        GridId = "ProductWarrantyReportGrid",
                        ListHeading = selectedTab.Title,
                        FilterPartial = "_ProductWarrantyReportFilterBar",
                        FilterModel = new ProductWarrantyReportFilterBarViewModel
                        {
                            Products = productList,
                            ProductID = ParseLong(query, "ProductID")
                        }
                    };
                }

                case ReportRoutes.Rewards:
                {
                    return new CrudIndexViewModel
                    {
                        GridId = "RewardReportGrid",
                        ListHeading = selectedTab.Title,
                        FilterPartial = "_RewardReportFilterBar"
                    };
                }

                case ReportRoutes.UserTransactionDetails:
                case ReportRoutes.ProductWarrantyTransactionDetails:
                case ReportRoutes.RewardTransactionDetails:
                {
                    var productList = await products.GetAll();
                    var cardDetails = selectedTab.Key == ReportRoutes.ProductWarrantyTransactionDetails;
                    var rewardsOnly = selectedTab.Key == ReportRoutes.RewardTransactionDetails;
                    var gridId = cardDetails ? "ProductCardDetailGrid" : "ReportActivityGrid";

                    return new CrudIndexViewModel
                    {
                        GridId = gridId,
                        ListHeading = selectedTab.Title,
                        FilterPartial = "_TransactionFilterBar",
                        FilterModel = new ReportActivityFilterBarViewModel
                        {
                            Url = ReportRoutes.GridPath(selectedTab.Key),
                            Target = "#" + gridId,
                            Products = productList,
                            CardDetails = cardDetails,
                            RewardsOnly = rewardsOnly,
                            UserID = QueryValue(query, "UserID"),
                            ProductID = ParseLong(query, "ProductID"),
                            WarrantyCardID = ParseLong(query, "WarrantyCardID"),
                            RewardRequestID = ParseInt(query, "RewardRequestID"),
                            RewardCatalogID = ParseInt(query, "RewardCatalogID")
                        }
                    };
                }

                default:
                    throw new InvalidOperationException(
                        "No report page configuration exists for tab: " + selectedTab.Key);
            }
        }

        private static string? QueryValue(IQueryCollection query, string name)
        {
            if (!query.TryGetValue(name, out var values))
            {
                return null;
            }

            var value = values.FirstOrDefault();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static long? ParseLong(IQueryCollection query, string name)
        {
            var value = QueryValue(query, name);
            return long.TryParse(value, out var parsed) && parsed > 0 ? parsed : null;
        }

        private static int? ParseInt(IQueryCollection query, string name)
        {
            var value = QueryValue(query, name);
            return int.TryParse(value, out var parsed) && parsed > 0 ? parsed : null;
        }
    }
}
