using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IReportService
    {
        Task<ReportSearchResult<ReportPage<UserListItem>>> SearchUsers(UserSearchModel search);

        Task<ReportSearchResult<ReportPage<ProductWarrantyReportRow>>>
            SearchProductWarranty(ProductWarrantyReportSearchModel search);

        Task<ReportSearchResult<ReportPage<RewardPopularityRow>>> SearchRewards(
            string? fromJalali,
            string? toJalali,
            int pageIndex);

        Task<ReportSearchResult<ReportPage<ReportActivityItem>>>
            SearchUserTransactions(ReportActivitySearchModel search);

        Task<ReportSearchResult<ReportPage<ProductCardDetailItem>>>
            SearchProductWarrantyTransactions(ReportActivitySearchModel search);

        Task<ReportSearchResult<ReportPage<ReportActivityItem>>>
            SearchRewardTransactions(ReportActivitySearchModel search);

        Task<ReportSearchResult<ReportPage<ProductPopularityRow>>> SearchProducts(
            int? year,
            int? month,
            int pageIndex);

        Task<ReportSearchResult<ReportPage<WarrantyProductStatusRow>>> SearchWarranty(
            long? productId,
            string? fromJalali,
            string? toJalali,
            int pageIndex);
    }
}
