using Application.Services;
using DataAccess.Services;
using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.User;
using Framework.Common;
using Framework.Common.Extensions;

namespace ApplicationService.Services
{
    public class ReportService : IReportService
    {
        public const int DefaultPageSize = 50;

        private readonly IReportRepository reports;
        private readonly IUserService users;

        public ReportService(IReportRepository reports, IUserService users)
        {
            this.reports = reports;
            this.users = users;
        }

        public async Task<ReportSearchResult<ReportPage<UserListItem>>> SearchUsers(UserSearchModel search)
        {
            var result = new ReportSearchResult<ReportPage<UserListItem>>("SearchReportUsers");

            var validation = ReportSearchValidator.ValidateDateRange(search.CardFromJalali, search.CardToJalali)
                ?? ReportSearchValidator.ValidateNumberRange(search.PointsFrom, search.PointsTo);

            if (validation != null)
            {
                result.Operation.ToFailed(validation, "InvalidFilter");
                return result;
            }

            try
            {
                PreparePagination(search);
                search.CardFrom = search.CardFromJalali.ToGregorianDate();
                search.CardTo = search.CardToJalali.ToGregorianDate();

                var userResult = await users.Search(search);
                var pagination = userResult.sm ?? search;

                result.Data = new ReportPage<UserListItem>
                {
                    Items = userResult.userList ?? new List<UserListItem>(),
                    PageIndex = pagination.PageIndex,
                    PageSize = pagination.PageSize,
                    RecordCount = pagination.RecordCount
                };

                result.Operation.ToSuccess("جستجو با موفقیت انجام شد.");
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                result.Operation.ToFailed(
                    "دریافت گزارش ناموفق بود. لطفاً دوباره تلاش کنید.",
                    "ReportSearchFailed");
                return result;
            }
        }

        public async Task<ReportSearchResult<ReportPage<ProductWarrantyReportRow>>> SearchProductWarranty(
            ProductWarrantyReportSearchModel search)
        {
            var result = new ReportSearchResult<ReportPage<ProductWarrantyReportRow>>("SearchProductWarrantyReport");

            var validation = ReportSearchValidator.ValidateDateRange(search.FromJalali, search.ToJalali)
                ?? ReportSearchValidator.ValidateNumberRange(search.CountFrom, search.CountTo)
                ?? ReportSearchValidator.ValidateNumberRange(search.PointsFrom, search.PointsTo)
                ?? ReportSearchValidator.ValidateIds(search.ProductID);

            if (validation != null)
            {
                result.Operation.ToFailed(validation, "InvalidFilter");
                return result;
            }

            try
            {
                PreparePagination(search);
                search.FromUtc = search.FromJalali.JalaliStartOfDayUtc();
                search.ToUtcExclusive = search.ToJalali.JalaliEndExclusiveUtc();

                result.Data = await reports.SearchProductWarranty(search);
                result.Operation.ToSuccess("جستجو با موفقیت انجام شد.");
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                result.Operation.ToFailed(
                    "دریافت گزارش ناموفق بود. لطفاً دوباره تلاش کنید.",
                    "ReportSearchFailed");
                return result;
            }
        }

        public async Task<ReportSearchResult<ReportPage<RewardPopularityRow>>> SearchRewards(
            string? fromJalali,
            string? toJalali,
            int pageIndex)
        {
            var result = new ReportSearchResult<ReportPage<RewardPopularityRow>>("SearchRewardsReport");

            var validation = ReportSearchValidator.ValidateDateRange(fromJalali, toJalali);
            if (validation != null)
            {
                result.Operation.ToFailed(validation, "InvalidFilter");
                return result;
            }

            try
            {
                result.Data = await reports.SearchRewardPopularity(
                    fromJalali.ToGregorianDate(),
                    toJalali.ToGregorianDate(),
                    pageIndex,
                    DefaultPageSize);

                result.Operation.ToSuccess("جستجو با موفقیت انجام شد.");
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                result.Operation.ToFailed(
                    "دریافت گزارش ناموفق بود. لطفاً دوباره تلاش کنید.",
                    "ReportSearchFailed");
                return result;
            }
        }

        public async Task<ReportSearchResult<ReportPage<ReportActivityItem>>> SearchUserTransactions(
            ReportActivitySearchModel search)
        {
            var result = new ReportSearchResult<ReportPage<ReportActivityItem>>("SearchUserTransactions");

            var validation = ReportSearchValidator.ValidateActivitySearch(search, false);
            if (validation != null)
            {
                result.Operation.ToFailed(validation, "InvalidFilter");
                return result;
            }

            try
            {
                PrepareActivitySearch(search);
                search.RewardsOnly = false;

                result.Data = await reports.SearchActivities(search);
                result.Operation.ToSuccess("جستجو با موفقیت انجام شد.");
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                result.Operation.ToFailed(
                    "دریافت گزارش ناموفق بود. لطفاً دوباره تلاش کنید.",
                    "ReportSearchFailed");
                return result;
            }
        }

        public async Task<ReportSearchResult<ReportPage<ProductCardDetailItem>>> SearchProductWarrantyTransactions(
            ReportActivitySearchModel search)
        {
            var result = new ReportSearchResult<ReportPage<ProductCardDetailItem>>("SearchProductWarrantyTransactions");

            var validation = ReportSearchValidator.ValidateDateRange(search.FromJalali, search.ToJalali)
                ?? ReportSearchValidator.ValidateIds(search.ProductID, search.WarrantyCardID);

            if (validation != null)
            {
                result.Operation.ToFailed(validation, "InvalidFilter");
                return result;
            }

            try
            {
                PrepareActivitySearch(search);
                result.Data = await reports.SearchProductCards(search);
                result.Operation.ToSuccess("جستجو با موفقیت انجام شد.");
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                result.Operation.ToFailed(
                    "دریافت گزارش ناموفق بود. لطفاً دوباره تلاش کنید.",
                    "ReportSearchFailed");
                return result;
            }
        }

        public async Task<ReportSearchResult<ReportPage<ReportActivityItem>>> SearchRewardTransactions(
            ReportActivitySearchModel search)
        {
            var result = new ReportSearchResult<ReportPage<ReportActivityItem>>("SearchRewardTransactions");

            var validation = ReportSearchValidator.ValidateActivitySearch(search, true);
            if (validation != null)
            {
                result.Operation.ToFailed(validation, "InvalidFilter");
                return result;
            }

            try
            {
                PrepareActivitySearch(search);
                search.RewardsOnly = true;

                result.Data = await reports.SearchActivities(search);
                result.Operation.ToSuccess("جستجو با موفقیت انجام شد.");
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                result.Operation.ToFailed(
                    "دریافت گزارش ناموفق بود. لطفاً دوباره تلاش کنید.",
                    "ReportSearchFailed");
                return result;
            }
        }

        public async Task<ReportSearchResult<ReportPage<ProductPopularityRow>>> SearchProducts(
            int? year,
            int? month,
            int pageIndex)
        {
            var result = new ReportSearchResult<ReportPage<ProductPopularityRow>>("SearchProductsReport");

            var validation = ReportSearchValidator.ValidateProductPopularity(year, month);
            if (validation != null)
            {
                result.Operation.ToFailed(validation, "InvalidFilter");
                return result;
            }

            try
            {
                result.Data = await reports.SearchProductPopularity(year, month, pageIndex, DefaultPageSize);
                result.Operation.ToSuccess("جستجو با موفقیت انجام شد.");
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                result.Operation.ToFailed(
                    "دریافت گزارش ناموفق بود. لطفاً دوباره تلاش کنید.",
                    "ReportSearchFailed");
                return result;
            }
        }

        public async Task<ReportSearchResult<ReportPage<WarrantyProductStatusRow>>> SearchWarranty(
            long? productId,
            string? fromJalali,
            string? toJalali,
            int pageIndex)
        {
            var result = new ReportSearchResult<ReportPage<WarrantyProductStatusRow>>("SearchWarrantyReport");

            var validation = ReportSearchValidator.ValidateDateRange(fromJalali, toJalali)
                ?? ReportSearchValidator.ValidateIds(productId);

            if (validation != null)
            {
                result.Operation.ToFailed(validation, "InvalidFilter");
                return result;
            }

            try
            {
                result.Data = await reports.SearchWarrantyByProduct(
                    productId,
                    fromJalali.ToGregorianDate(),
                    toJalali.ToGregorianDate(),
                    pageIndex,
                    DefaultPageSize);

                result.Operation.ToSuccess("جستجو با موفقیت انجام شد.");
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                result.Operation.ToFailed(
                    "دریافت گزارش ناموفق بود. لطفاً دوباره تلاش کنید.",
                    "ReportSearchFailed");
                return result;
            }
        }

        private static void PreparePagination(PageModel search)
        {
            search.PageSize = DefaultPageSize;
            search.PageIndex = Math.Clamp(search.PageIndex, 0, int.MaxValue / DefaultPageSize);
        }

        private static void PrepareActivitySearch(ReportActivitySearchModel search)
        {
            PreparePagination(search);
            search.FromUtc = search.FromJalali.JalaliStartOfDayUtc();
            search.ToUtcExclusive = search.ToJalali.JalaliEndExclusiveUtc();
        }
    }
}
