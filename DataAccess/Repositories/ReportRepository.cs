using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Product;
using DomainModel.ViewModels.Reports;
using Framework.Common;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly GolpaMotorDbContext db;

        public ReportRepository(GolpaMotorDbContext db)
        {
            this.db = db;
        }

        public async Task<List<WarrantyProductStatusRow>> GetWarrantyByProduct(long? productId, DateTime? from, DateTime? to)
        {
            var today = DateTime.Today;
            var query = db.WarrantyCards.AsQueryable();
            if (productId.HasValue && productId.Value > 0)
                query = query.Where(x => x.ProductID == productId.Value);

            var cards = await query
                .Select(x => new
                {
                    x.Product.ProductName,
                    x.IsRegistered,
                    RegisteredAt = x.CardRegistrations.Select(r => (DateTime?)r.CreatedAt).Min(),
                    x.ValidityMonths
                })
                .ToListAsync();

            if (from.HasValue || to.HasValue)
            {
                var start = from ?? DateTime.MinValue;
                var end = to?.Date.AddDays(1) ?? DateTime.MaxValue;
                cards = cards
                    .Where(x => x.RegisteredAt.HasValue && x.RegisteredAt.Value >= start && x.RegisteredAt.Value < end)
                    .ToList();
            }

            return cards
                .GroupBy(x => x.ProductName)
                .Select(g =>
                {
                    var remaining = g.Select(x => WarrantyValidity.RemainingDays(x.RegisteredAt, x.ValidityMonths, today));
                    return new WarrantyProductStatusRow
                    {
                        ProductName = g.Key,
                        TotalCards = g.Count(),
                        Registered = g.Count(x => x.IsRegistered),
                        Unregistered = g.Count(x => !x.IsRegistered),
                        Expired = remaining.Count(d => d.HasValue && d.Value < 0),
                        ExpiringSoon = remaining.Count(d => d.HasValue && d.Value >= 0 && d.Value <= 10)
                    };
                })
                .OrderByDescending(x => x.Registered)
                .ToList();
        }

        public async Task<List<ProductPopularityRow>> GetProductPopularity(int? jalaliYear, int? jalaliMonth)
        {
            var rows = await db.CardRegistrations
                .Select(x => new
                {
                    ProductName = x.WarrantyCard.Product.ProductName,
                    x.CreatedAt
                })
                .ToListAsync();

            return rows
                .Select(x => new
                {
                    x.ProductName,
                    Year = PersianDate.GetYear(x.CreatedAt),
                    Month = PersianDate.GetMonth(x.CreatedAt)
                })
                .Where(x => (!jalaliYear.HasValue || x.Year == jalaliYear.Value) &&
                             (!jalaliMonth.HasValue || x.Month == jalaliMonth.Value))
                .GroupBy(x => new { x.ProductName, x.Year, x.Month })
                .Select(g => new ProductPopularityRow
                {
                    ProductName = g.Key.ProductName,
                    JalaliYear = g.Key.Year,
                    JalaliMonth = g.Key.Month,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();
        }

        public async Task<List<RewardPopularityRow>> GetRewardPopularity(DateTime? from, DateTime? to)
        {
            var query = db.RewardRequests.AsQueryable();
            if (from.HasValue)
                query = query.Where(x => x.RequestDate != null && x.RequestDate >= from.Value);
            if (to.HasValue)
            {
                var end = to.Value.Date.AddDays(1);
                query = query.Where(x => x.RequestDate != null && x.RequestDate < end);
            }

            var items = await query
                .Select(x => new
                {
                    Title = x.RewardCatalog.Title,
                    Status = x.RewardDeliveryStatus.Title,
                    x.IsComplete
                })
                .ToListAsync();

            return items
                .GroupBy(x => x.Title)
                .Select(g => new RewardPopularityRow
                {
                    Title = g.Key,
                    RequestCount = g.Count(),
                    ApprovedCount = g.Count(x => x.Status == RewardStatusTitles.Approved || x.IsComplete),
                    RejectedCount = g.Count(x => x.Status == RewardStatusTitles.Rejected),
                    PendingCount = g.Count(x => x.Status == RewardStatusTitles.Pending)
                })
                .OrderByDescending(x => x.RequestCount)
                .ToList();
        }

        public async Task<List<NamedCountItem>> GetTopProducts(int take = 5)
        {
            return await db.CardRegistrations
                .GroupBy(x => x.WarrantyCard.Product.ProductName)
                .Select(g => new NamedCountItem { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<NamedCountItem>> GetTopRewards(int take = 5)
        {
            return await db.RewardRequests
                .GroupBy(x => x.RewardCatalog.Title)
                .Select(g => new NamedCountItem { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(take)
                .ToListAsync();
        }
    }
}
