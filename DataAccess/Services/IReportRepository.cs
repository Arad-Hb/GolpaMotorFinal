using DomainModel.ViewModels.Product;
using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.User;

namespace DataAccess.Services
{
    public interface IReportRepository
    {
        Task<List<WarrantyProductStatusRow>> GetWarrantyByProduct(long? productId, DateTime? from, DateTime? to);
        Task<List<ProductPopularityRow>> GetProductPopularity(int? jalaliYear, int? jalaliMonth);
        Task<List<RewardPopularityRow>> GetRewardPopularity(DateTime? from, DateTime? to);
        Task<List<NamedCountItem>> GetTopProducts(int take = 5);
        Task<List<NamedCountItem>> GetTopRewards(int take = 5);
    }
}
