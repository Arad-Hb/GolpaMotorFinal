using Application.Services;
using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.User;
using Framework.Common;
using Framework.Common.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationService.Services
{
    public class ReportService : IReportService
    {
        private const int DefaultPageSize = 50;

        private readonly IReportRepository reports;
        private readonly IUserService users;
        //private readonly ILogger<ReportService> logger;

        public ReportService(IReportRepository reports,IUserService users)
            //ILogger<ReportService> logger)
        {
            this.reports = reports;
            this.users = users;
            //this.logger = logger;
        }

        public Task<ReportSearchResult<ReportPage<UserListItem>>> SearchUsers(UserSearchModel search)
        {
            return ExecuteSearch("SearchReportUsers",
                () =>
                    ValidateDateRange(search.CardFromJalali,search.CardToJalali)
                    ?? ValidateNumberRange(search.PointsFrom,search.PointsTo),
                async () =>
                {
                    PreparePagination(search);

                    search.CardFrom =search.CardFromJalali.ToGregorianDate();

                    search.CardTo =search.CardToJalali.ToGregorianDate();

                    var userResult = await users.Search(search);
                    var pagination = userResult.sm ?? search;

                    return new ReportPage<UserListItem>
                    {
                        Items = userResult.userList ?? new List<UserListItem>(),

                        PageIndex = pagination.PageIndex,
                        PageSize = pagination.PageSize,
                        RecordCount = pagination.RecordCount
                    };
                });
        }

        public Task<ReportSearchResult<ReportPage<ProductWarrantyReportRow>>> SearchProductWarranty(ProductWarrantyReportSearchModel search)
        {
            return ExecuteSearch("SearchProductWarrantyReport",
                () =>
                    ValidateDateRange(search.FromJalali,search.ToJalali)
                    ?? ValidateNumberRange(search.CountFrom,search.CountTo)
                    ?? ValidateNumberRange(search.PointsFrom,search.PointsTo)
                    ?? ValidateIds(search.ProductID),
                () =>
                {
                    PreparePagination(search);

                    search.FromUtc =search.FromJalali.JalaliStartOfDayUtc();

                    search.ToUtcExclusive =search.ToJalali.JalaliEndExclusiveUtc();

                    return reports.SearchProductWarranty(search);
                });
        }

        public Task<ReportSearchResult<ReportPage<RewardPopularityRow>>> SearchRewards(string? fromJalali,string? toJalali,int pageIndex)
        {
            return ExecuteSearch("SearchRewardsReport",() => ValidateDateRange(fromJalali, toJalali),
                async () =>
                {
                    var rows = await reports.GetRewardPopularity(
                        fromJalali.ToGregorianDate(),
                        toJalali.ToGregorianDate());

                    return CreatePage(rows, pageIndex);
                });
        }

        public Task<ReportSearchResult<ReportPage<ReportActivityItem>>> SearchUserTransactions(ReportActivitySearchModel search)
        {
            return ExecuteSearch("SearchUserTransactions",() => ValidateActivitySearch(search, false),
                () =>
                {
                    PrepareActivitySearch(search);

                    // The service controls the scope, not the query string.
                    search.RewardsOnly = false;

                    return reports.SearchActivities(search);
                });
        }

        public Task<ReportSearchResult<ReportPage<ProductCardDetailItem>>> SearchProductWarrantyTransactions(ReportActivitySearchModel search)
        {
            return ExecuteSearch("SearchProductWarrantyTransactions",() =>
                    ValidateDateRange(search.FromJalali,search.ToJalali)?? ValidateIds(search.ProductID,search.WarrantyCardID),
                () =>
                {
                    PrepareActivitySearch(search);

                    return reports.SearchProductCards(search);
                });
        }

        public Task<ReportSearchResult<ReportPage<ReportActivityItem>>> SearchRewardTransactions(ReportActivitySearchModel search)
        {
            return ExecuteSearch("SearchRewardTransactions",() => ValidateActivitySearch(search, true),
                () =>
                {
                    PrepareActivitySearch(search);

                    search.RewardsOnly = true;

                    return reports.SearchActivities(search);
                });
        }

        public Task<ReportSearchResult<ReportPage<ProductPopularityRow>>> SearchProducts(int? year,int? month,int pageIndex)
        {
            return ExecuteSearch("SearchProductsReport",() =>
                {
                    if (month.HasValue && (month.Value < 1 || month.Value > 12))
                    {
                        return "ماه واردشده نامعتبر است.";
                    }

                    if (year.HasValue && (year.Value < 1 || year.Value > 9377))
                    {
                        return "سال واردشده نامعتبر است.";
                    }

                    return null;
                },
                async () =>
                {
                    var rows = await reports.GetProductPopularity(year,month);

                    return CreatePage(rows, pageIndex);
                });
        }

        public Task<ReportSearchResult<ReportPage<WarrantyProductStatusRow>>> SearchWarranty(long? productId,string? fromJalali,string? toJalali,int pageIndex)
        {
            return ExecuteSearch("SearchWarrantyReport",() =>ValidateDateRange(fromJalali, toJalali)?? ValidateIds(productId),
                async () =>
                {
                    var rows = await reports.GetWarrantyByProduct(
                        productId,
                        fromJalali.ToGregorianDate(),
                        toJalali.ToGregorianDate());

                    return CreatePage(rows, pageIndex);
                });
        }

        // All report searches follow the same success/failure contract.
        private async Task<ReportSearchResult<T>> ExecuteSearch<T>(string operationName,Func<string?> validate,Func<Task<T>> search)
        {
            var result = new ReportSearchResult<T>(operationName);

            try
            {
                var validationMessage = validate();

                if (validationMessage != null)
                {
                    result.Operation.ToFailed(validationMessage,"InvalidFilter");

                    return result;
                }

                result.Data = await search();

                result.Operation.ToSuccess("جستجو با موفقیت انجام شد.");

                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                //logger.LogError(
                //    exception,
                //    "Report search failed: {OperationName}",
                //    operationName);

                result.Operation.ToFailed("دریافت گزارش ناموفق بود. لطفاً دوباره تلاش کنید.","ReportSearchFailed");

                return result;
            }
        }

        private static void PreparePagination(PageModel search)
        {
            search.PageSize = DefaultPageSize;

            search.PageIndex = Math.Clamp(search.PageIndex,0,int.MaxValue / DefaultPageSize);
        }

        private static void PrepareActivitySearch(ReportActivitySearchModel search)
        {
            PreparePagination(search);

            search.FromUtc =search.FromJalali.JalaliStartOfDayUtc();

            search.ToUtcExclusive =search.ToJalali.JalaliEndExclusiveUtc();
        }

        private static string? ValidateDateRange(string? fromJalali, string? toJalali)
        {
            var from = fromJalali.ToGregorianDate();
            var to = toJalali.ToGregorianDate();

            if (!string.IsNullOrWhiteSpace(fromJalali) && !from.HasValue)
            {
                return "تاریخ شروع نامعتبر است.";
            }

            if (!string.IsNullOrWhiteSpace(toJalali) && !to.HasValue)
            {
                return "تاریخ پایان نامعتبر است.";
            }

            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {
                return "تاریخ شروع نباید بعد از تاریخ پایان باشد.";
            }

            return null;
        }

        private static string? ValidateNumberRange(int? from,int? to)
        {
            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {
                return "حداقل بازه نباید بیشتر از حداکثر باشد.";
            }

            return null;
        }

        private static string? ValidateIds(params long?[] ids)
        {
            foreach (var id in ids)
            {
                if (id.HasValue && id.Value <= 0)
                {
                    return "شناسه فیلتر نامعتبر است.";
                }
            }

            return null;
        }

        private static string? ValidateActivitySearch(ReportActivitySearchModel search,bool rewardsOnly)
        {
            var error =ValidateDateRange(search.FromJalali,search.ToJalali)
                ?? ValidateNumberRange(search.PointsFrom,search.PointsTo)
                ?? ValidateIds(search.ProductID,search.WarrantyCardID,search.RewardRequestID,search.RewardCatalogID);

            if (error != null)
            {
                return error;
            }

            var allowedActivities = new[]
            {
                ReportActivityTypes.CardRegistered,
                ReportActivityTypes.RewardRequested,
                ReportActivityTypes.RewardApproved,
                ReportActivityTypes.RewardRejected
            };

            if (!string.IsNullOrWhiteSpace(search.ActivityType) && !allowedActivities.Contains(search.ActivityType))
            {
                return "نوع رویداد نامعتبر است.";
            }

            if (rewardsOnly && search.ActivityType == ReportActivityTypes.CardRegistered)
            {
                return "فعال‌سازی کارت مربوط به گزارش پاداش نیست.";
            }

            var allowedStatuses = new[]
            {
                RewardStatusTitles.Pending,
                RewardStatusTitles.Approved,
                RewardStatusTitles.Rejected,
                RewardStatusTitles.Paid,
                RewardStatusTitles.Cancelled
            };

            if (!string.IsNullOrWhiteSpace(search.RewardStatus) &&
                !allowedStatuses.Contains(search.RewardStatus))
            {
                return "وضعیت پاداش نامعتبر است.";
            }

            return null;
        }

        private static ReportPage<T> CreatePage<T>(List<T> rows,int pageIndex)
        {
            var pageCount = (int)Math.Ceiling(rows.Count / (double)DefaultPageSize);

            var lastPageIndex = Math.Max(0, pageCount - 1);

            pageIndex = Math.Clamp(pageIndex,0,lastPageIndex);

            return new ReportPage<T>
            {
                Items = rows.Skip(pageIndex * DefaultPageSize).Take(DefaultPageSize).ToList(),
                PageIndex = pageIndex,
                PageSize = DefaultPageSize,
                RecordCount = rows.Count
            };
        }
    }
}
