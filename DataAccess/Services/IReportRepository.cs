using DomainModel.ViewModels.Product;
using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.Reward;
using DomainModel.ViewModels.User;

namespace DataAccess.Services
{
    public interface IReportRepository
    {
        Task<List<WarrantyProductStatusRow>> GetWarrantyByProduct(long? productId, DateTime? from, DateTime? to);
        Task<List<ProductPopularityRow>> GetProductPopularity(int? jalaliYear, int? jalaliMonth);
        Task<List<RewardPopularityRow>> GetRewardPopularity(DateTime? from, DateTime? to);
        Task<ReportPage<WarrantyProductStatusRow>> SearchWarrantyByProduct(
            long? productId,
            DateTime? from,
            DateTime? to,
            int pageIndex,
            int pageSize);
        Task<ReportPage<ProductPopularityRow>> SearchProductPopularity(
            int? jalaliYear,
            int? jalaliMonth,
            int pageIndex,
            int pageSize);
        Task<ReportPage<RewardPopularityRow>> SearchRewardPopularity(
            DateTime? from,
            DateTime? to,
            int pageIndex,
            int pageSize);
        Task<List<NamedCountItem>> GetTopProducts(int take = 5);
        Task<List<NamedCountItem>> GetTopRewards(int take = 5);
        Task<AdminDashboardSummary> GetAdminDashboardSummary();
        Task<List<RewardRequestListItem>> GetRecentPendingRewardRequests(int take = 8);
        Task<ReportPage<DashboardRegistrarItem>> GetDashboardRegistrars(int pageIndex, int pageSize);
        Task<List<DashboardWarrantyAlertItem>> GetWarrantyExpiryAlerts(int take = 10, int withinDays = 30);
        Task<ReportPage<ProductWarrantyReportRow>> SearchProductWarranty(ProductWarrantyReportSearchModel search);
        Task<ReportPage<ReportActivityItem>> SearchActivities(ReportActivitySearchModel search);
        Task<ReportPage<ProductCardDetailItem>> SearchProductCards(ReportActivitySearchModel search);
    }
}
