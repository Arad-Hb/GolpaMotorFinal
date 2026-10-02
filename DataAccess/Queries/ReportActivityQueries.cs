using DomainModel.Models;
using DomainModel.ViewModels.Reports;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Queries
{
    public static class ReportActivityQueries
    {
        public static IQueryable<ReportActivityLog> Base(GolpaMotorDbContext db) =>
            db.ReportActivityLogs.AsNoTracking();

        public static IQueryable<ReportActivityLog> ApplySearch(IQueryable<ReportActivityLog> query,ReportActivitySearchModel search)
        {
            if (search.RewardsOnly)
            {
                query = query.Where(activity =>
                    activity.ActivityType == ReportActivityTypes.RewardRequested ||
                    activity.ActivityType == ReportActivityTypes.RewardApproved ||
                    activity.ActivityType == ReportActivityTypes.RewardRejected);
            }

            if (search.ProductID.HasValue && search.ProductID.Value > 0)
            {
                query = query.Where(x => x.ProductID == search.ProductID.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.UserID))
            {
                query = query.Where(x => x.UserID == search.UserID);
            }

            if (search.WarrantyCardID.HasValue && search.WarrantyCardID.Value > 0)
            {
                query = query.Where(x => x.WarrantyCardID == search.WarrantyCardID.Value);
            }

            if (search.RewardRequestID.HasValue && search.RewardRequestID.Value > 0)
            {
                query = query.Where(x => x.RewardRequestID == search.RewardRequestID.Value);
            }

            if (search.RewardCatalogID.HasValue && search.RewardCatalogID.Value > 0)
            {
                query = query.Where(x =>
                    x.RewardRequest != null &&
                    x.RewardRequest.RewardCatalogID == search.RewardCatalogID.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.ActivityType))
            {
                query = query.Where(x => x.ActivityType == search.ActivityType);
            }

            if (!string.IsNullOrWhiteSpace(search.RewardStatus))
            {
                query = query.Where(x => x.StatusTitle == search.RewardStatus);
            }

            if (search.PointsFrom.HasValue)
            {
                query = query.Where(x => x.PointsDelta >= search.PointsFrom.Value);
            }

            if (search.PointsTo.HasValue)
            {
                query = query.Where(x => x.PointsDelta <= search.PointsTo.Value);
            }

            if (search.FromUtc.HasValue)
            {
                query = query.Where(x => x.OccurredAtUtc >= search.FromUtc.Value);
            }

            if (search.ToUtcExclusive.HasValue)
            {
                query = query.Where(x => x.OccurredAtUtc < search.ToUtcExclusive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.SearchTerm))
            {
                var term = search.SearchTerm.Trim();
                query = query.Where(x =>
                    x.User.FirstName!.Contains(term) ||
                    x.User.LastName!.Contains(term) ||
                    x.User.PhoneNumber!.Contains(term) ||
                    (x.Product != null && x.Product.ProductName.Contains(term)) ||
                    (x.WarrantyCard != null &&
                        (x.WarrantyCard.SerialNumber.Contains(term) ||
                         x.WarrantyCard.ScratchedCode.Contains(term))));
            }

            return query;
        }
    }
}
