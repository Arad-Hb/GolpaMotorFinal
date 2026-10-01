using DomainModel.Models;
using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.Reward;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Queries
{
    public static class RewardReportQueries
    {
        public static IQueryable<RewardRequest> ApplyRequestDateRange(
            IQueryable<RewardRequest> query,
            DateTime? from,
            DateTime? to)
        {
            if (from.HasValue)
            {
                query = query.Where(x => x.RequestDate != null && x.RequestDate >= from.Value);
            }

            if (to.HasValue)
            {
                var end = to.Value.Date.AddDays(1);
                query = query.Where(x => x.RequestDate != null && x.RequestDate < end);
            }

            return query;
        }

        public static async Task<List<RewardPopularityRow>> LoadPopularityRows(
            GolpaMotorDbContext db,
            DateTime? from,
            DateTime? to)
        {
            var query = ApplyRequestDateRange(RewardRequestQueries.All(db), from, to);

            var items = await query
                .Select(x => new
                {
                    x.RewardCatalogID,
                    Title = x.RewardCatalog.Title,
                    Status = x.RewardDeliveryStatus.Title,
                    x.IsComplete
                })
                .ToListAsync();

            return items
                .GroupBy(x => new { x.RewardCatalogID, x.Title })
                .Select(g => new RewardPopularityRow
                {
                    RewardCatalogID = g.Key.RewardCatalogID,
                    Title = g.Key.Title,
                    RequestCount = g.Count(),
                    ApprovedCount = g.Count(x => x.Status == RewardStatusTitles.Approved || x.IsComplete),
                    RejectedCount = g.Count(x => x.Status == RewardStatusTitles.Rejected),
                    PendingCount = g.Count(x => x.Status == RewardStatusTitles.Pending)
                })
                .OrderByDescending(x => x.RequestCount)
                .ToList();
        }
    }
}
