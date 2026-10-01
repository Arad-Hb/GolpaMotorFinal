using DomainModel.ViewModels.Reports;

namespace GolpaMotorFinal.Helpers
{
    public static class ReportRoutes
    {
        public const string Users = "users";
        public const string ProductWarranty = "productwarranty";
        public const string Rewards = "rewards";
        public const string UserTransactionDetails = "userstransactiondetails";
        public const string ProductWarrantyTransactionDetails = "productwarrantytransactiondetails";
        public const string RewardTransactionDetails = "rewardstransactiondetails";

        public static string TabPath(string tabKey) => "/reports/" + tabKey;

        public static string TabUrl(string tabKey, object? query = null)
        {
            var path = TabPath(tabKey);
            return query == null ? path : FilterUrl.Combine(path, query);
        }

        public static string GridPath(string tabKey) => "/reports/grid/" + tabKey;

        public static string UserTransactions(string userId) =>
            TabUrl(UserTransactionDetails, new { UserID = userId });

        public static string ProductWarrantySummary(long productId) =>
            TabUrl(ProductWarranty, new { ProductID = productId });

        public static string ProductCardDetails(long? productId = null, long? warrantyCardId = null, string? userId = null) =>
            TabUrl(ProductWarrantyTransactionDetails, new
            {
                ProductID = productId,
                WarrantyCardID = warrantyCardId,
                UserID = userId
            });

        public static string RewardCatalog(int rewardCatalogId) =>
            TabUrl(RewardTransactionDetails, new { RewardCatalogID = rewardCatalogId });

        public static string RewardRequest(int rewardRequestId) =>
            TabUrl(RewardTransactionDetails, new { RewardRequestID = rewardRequestId });

        public static string ProductFromActivity(long productId) =>
            ProductCardDetails(productId);

        public static string ActivitySearchPagerUrl(string gridTabKey, ReportActivitySearchModel search) =>
            FilterUrl.Combine(GridPath(gridTabKey), new
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
}
