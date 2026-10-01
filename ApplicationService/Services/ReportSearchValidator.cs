using DomainModel.Models;
using DomainModel.ViewModels.Reports;
using Framework.Common.Extensions;

namespace ApplicationService.Services
{
    public static class ReportSearchValidator
    {
        public static string? ValidateDateRange(string? fromJalali, string? toJalali)
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

        public static string? ValidateNumberRange(int? from, int? to)
        {
            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {
                return "حداقل بازه نباید بیشتر از حداکثر باشد.";
            }

            return null;
        }

        public static string? ValidateIds(params long?[] ids)
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

        public static string? ValidateRewardIds(int? rewardRequestId, int? rewardCatalogId)
        {
            if (rewardRequestId.HasValue && rewardRequestId.Value <= 0)
            {
                return "شناسه فیلتر نامعتبر است.";
            }

            if (rewardCatalogId.HasValue && rewardCatalogId.Value <= 0)
            {
                return "شناسه فیلتر نامعتبر است.";
            }

            return null;
        }

        public static string? ValidateProductPopularity(int? year, int? month)
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
        }

        public static string? ValidateActivitySearch(ReportActivitySearchModel search, bool rewardsOnly)
        {
            var error = ValidateDateRange(search.FromJalali, search.ToJalali)
                ?? ValidateNumberRange(search.PointsFrom, search.PointsTo)
                ?? ValidateIds(search.ProductID, search.WarrantyCardID)
                ?? ValidateRewardIds(search.RewardRequestID, search.RewardCatalogID);

            if (error != null)
            {
                return error;
            }

            if (!string.IsNullOrWhiteSpace(search.ActivityType) &&
                search.ActivityType is not (
                    ReportActivityTypes.CardRegistered or
                    ReportActivityTypes.RewardRequested or
                    ReportActivityTypes.RewardApproved or
                    ReportActivityTypes.RewardRejected))
            {
                return "نوع رویداد نامعتبر است.";
            }

            if (rewardsOnly && search.ActivityType == ReportActivityTypes.CardRegistered)
            {
                return "فعال‌سازی کارت مربوط به گزارش پاداش نیست.";
            }

            if (!string.IsNullOrWhiteSpace(search.RewardStatus) &&
                search.RewardStatus is not (
                    RewardStatusTitles.Pending or
                    RewardStatusTitles.Approved or
                    RewardStatusTitles.Rejected or
                    RewardStatusTitles.Paid or
                    RewardStatusTitles.Cancelled))
            {
                return "وضعیت پاداش نامعتبر است.";
            }

            return null;
        }
    }
}
