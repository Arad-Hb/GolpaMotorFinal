using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Product;
using DomainModel.ViewModels.Reports;
using Framework.Common;
using Framework.Common.Extensions;
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
                    Year = x.CreatedAt.GetPersianYear(),
                    Month = x.CreatedAt.GetPersianMonth()
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

        public async Task<ReportPage<ProductWarrantyReportRow>> SearchProductWarranty(
            ProductWarrantyReportSearchModel search)
        {
            search ??= new ProductWarrantyReportSearchModel();
            var pageIndex = Math.Max(0, search.PageIndex);
            var pageSize = search.PageSize <= 0 ? 50 : search.PageSize;

            var products = db.Products.AsNoTracking().Where(x => !x.IsDeleted);
            if (search.ProductID.HasValue && search.ProductID.Value > 0)
                products = products.Where(x => x.ProductID == search.ProductID.Value);
            if (!string.IsNullOrWhiteSpace(search.SearchTerm))
            {
                var term = search.SearchTerm.Trim();
                products = products.Where(x => x.ProductName.Contains(term));
            }

            var cards = db.WarrantyCards.AsNoTracking().AsQueryable();
            if (search.FromUtc.HasValue)
                cards = cards.Where(x => x.IssuedAtUtc >= search.FromUtc.Value);
            if (search.ToUtcExclusive.HasValue)
                cards = cards.Where(x => x.IssuedAtUtc < search.ToUtcExclusive.Value);
            if (search.IsRegistered.HasValue)
                cards = cards.Where(x => x.IsRegistered == search.IsRegistered.Value);

            var registrations = db.CardRegistrations.AsNoTracking().AsQueryable();
            if (search.FromUtc.HasValue)
                registrations = registrations.Where(x => x.CreatedAt >= search.FromUtc.Value);
            if (search.ToUtcExclusive.HasValue)
                registrations = registrations.Where(x => x.CreatedAt < search.ToUtcExclusive.Value);

            var query = products.Select(p => new ProductWarrantyReportRow
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                CreatedAtUtc = p.CreatedAtUtc,
                TotalCards = cards.Count(w => w.ProductID == p.ProductID),
                RegisteredCards = cards.Count(w => w.ProductID == p.ProductID && w.IsRegistered),
                UnregisteredCards = cards.Count(w => w.ProductID == p.ProductID && !w.IsRegistered),
                UniqueCustomers = registrations
                    .Where(r => r.WarrantyCard.ProductID == p.ProductID)
                    .Select(r => r.UserID)
                    .Distinct()
                    .Count(),
                AwardedPoints = registrations
                    .Where(r => r.WarrantyCard.ProductID == p.ProductID)
                    .Sum(r => (int?)(r.EarnedPionts != 0
                        ? r.EarnedPionts
                        : r.WarrantyCard.Product.ProductPoint)) ?? 0,
                CustomersWithRewardRequest = db.RewardRequests
                    .Where(rr => registrations.Any(r =>
                        r.WarrantyCard.ProductID == p.ProductID &&
                        r.UserID == rr.UserID))
                    .Select(rr => rr.UserID)
                    .Distinct()
                    .Count(),
                RewardRequestCount = db.RewardRequests.Count(rr =>
                    registrations.Any(r =>
                        r.WarrantyCard.ProductID == p.ProductID &&
                        r.UserID == rr.UserID)),
                RegistrantSettledPoints = db.Users
                    .Where(u => registrations.Any(r =>
                        r.WarrantyCard.ProductID == p.ProductID &&
                        r.UserID == u.Id))
                    .Sum(u => (int?)u.TotalSettledPoints) ?? 0,
                RegistrantRemainedPoints = db.Users
                    .Where(u => registrations.Any(r =>
                        r.WarrantyCard.ProductID == p.ProductID &&
                        r.UserID == u.Id))
                    .Sum(u => (int?)u.RemainedPoints) ?? 0
            });

            if (search.CountFrom.HasValue)
                query = query.Where(x => x.TotalCards >= search.CountFrom.Value);
            if (search.CountTo.HasValue)
                query = query.Where(x => x.TotalCards <= search.CountTo.Value);
            if (search.PointsFrom.HasValue)
                query = query.Where(x => x.AwardedPoints >= search.PointsFrom.Value);
            if (search.PointsTo.HasValue)
                query = query.Where(x => x.AwardedPoints <= search.PointsTo.Value);

            var recordCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.RegisteredCards)
                .ThenBy(x => x.ProductName)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach (var item in items)
                item.CustomersWithoutRewardRequest =
                    Math.Max(0, item.UniqueCustomers - item.CustomersWithRewardRequest);

            return new ReportPage<ProductWarrantyReportRow>
            {
                Items = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = recordCount
            };
        }

        public async Task<ReportPage<ReportActivityItem>> SearchActivities(
            ReportActivitySearchModel search)
        {
            search ??= new ReportActivitySearchModel();
            var pageIndex = Math.Max(0, search.PageIndex);
            var pageSize = search.PageSize <= 0 ? 50 : search.PageSize;
            var query = db.ReportActivityLogs.AsNoTracking().AsQueryable();

            if (search.ProductID.HasValue && search.ProductID.Value > 0)
                query = query.Where(x => x.ProductID == search.ProductID.Value);
            if (!string.IsNullOrWhiteSpace(search.UserID))
                query = query.Where(x => x.UserID == search.UserID);
            if (search.WarrantyCardID.HasValue && search.WarrantyCardID.Value > 0)
                query = query.Where(x => x.WarrantyCardID == search.WarrantyCardID.Value);
            if (search.RewardRequestID.HasValue && search.RewardRequestID.Value > 0)
                query = query.Where(x => x.RewardRequestID == search.RewardRequestID.Value);
            if (search.RewardCatalogID.HasValue && search.RewardCatalogID.Value > 0)
                query = query.Where(x =>
                    x.RewardRequest != null &&
                    x.RewardRequest.RewardCatalogID == search.RewardCatalogID.Value);
            if (!string.IsNullOrWhiteSpace(search.ActivityType))
                query = query.Where(x => x.ActivityType == search.ActivityType);
            if (!string.IsNullOrWhiteSpace(search.RewardStatus))
                query = query.Where(x => x.StatusTitle == search.RewardStatus);
            if (search.PointsFrom.HasValue)
                query = query.Where(x => x.PointsDelta >= search.PointsFrom.Value);
            if (search.PointsTo.HasValue)
                query = query.Where(x => x.PointsDelta <= search.PointsTo.Value);
            if (search.FromUtc.HasValue)
                query = query.Where(x => x.OccurredAtUtc >= search.FromUtc.Value);
            if (search.ToUtcExclusive.HasValue)
                query = query.Where(x => x.OccurredAtUtc < search.ToUtcExclusive.Value);
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

            var recordCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.OccurredAtUtc)
                .ThenByDescending(x => x.ReportActivityLogID)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(x => new ReportActivityItem
                {
                    ReportActivityLogID = x.ReportActivityLogID,
                    ActivityType = x.ActivityType,
                    OccurredAtUtc = x.OccurredAtUtc,
                    UserID = x.UserID,
                    UserName = ((x.User.FirstName ?? "") + " " + (x.User.LastName ?? "")).Trim(),
                    PhoneNumber = x.User.PhoneNumber,
                    ProductID = x.ProductID,
                    ProductName = x.Product != null ? x.Product.ProductName : null,
                    WarrantyCardID = x.WarrantyCardID,
                    SerialNumber = x.WarrantyCard != null ? x.WarrantyCard.SerialNumber : null,
                    ScratchedCode = x.WarrantyCard != null ? x.WarrantyCard.ScratchedCode : null,
                    RewardRequestID = x.RewardRequestID,
                    RewardTitle = x.RewardRequest != null ? x.RewardRequest.RewardCatalog.Title : null,
                    StatusTitle = x.StatusTitle,
                    PointsDelta = x.PointsDelta,
                    TotalEarnedPoints = x.TotalEarnedPoints,
                    TotalSettledPoints = x.TotalSettledPoints,
                    RemainedPoints = x.RemainedPoints,
                    AvailablePoints = x.AvailablePoints,
                    Description = x.Description
                })
                .ToListAsync();

            foreach (var item in items.Where(x => string.IsNullOrWhiteSpace(x.UserName)))
                item.UserName = item.PhoneNumber ?? "نامشخص";

            return new ReportPage<ReportActivityItem>
            {
                Items = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = recordCount
            };
        }

        public async Task<ReportPage<ProductCardDetailItem>> SearchProductCards(
            ReportActivitySearchModel search)
        {
            search ??= new ReportActivitySearchModel();
            var pageIndex = Math.Max(0, search.PageIndex);
            var pageSize = search.PageSize <= 0 ? 50 : search.PageSize;
            var query = db.WarrantyCards.AsNoTracking().AsQueryable();

            if (search.ProductID.HasValue && search.ProductID.Value > 0)
                query = query.Where(x => x.ProductID == search.ProductID.Value);
            if (search.WarrantyCardID.HasValue && search.WarrantyCardID.Value > 0)
                query = query.Where(x => x.WarrantyCardID == search.WarrantyCardID.Value);
            if (search.FromUtc.HasValue)
                query = query.Where(x => x.IssuedAtUtc >= search.FromUtc.Value);
            if (search.ToUtcExclusive.HasValue)
                query = query.Where(x => x.IssuedAtUtc < search.ToUtcExclusive.Value);
            if (!string.IsNullOrWhiteSpace(search.UserID))
                query = query.Where(x => x.CardRegistrations.Any(r => r.UserID == search.UserID));
            if (!string.IsNullOrWhiteSpace(search.SearchTerm))
            {
                var term = search.SearchTerm.Trim();
                query = query.Where(x =>
                    x.SerialNumber.Contains(term) ||
                    x.ScratchedCode.Contains(term) ||
                    x.Product.ProductName.Contains(term) ||
                    x.CardRegistrations.Any(r =>
                        r.User.PhoneNumber!.Contains(term) ||
                        r.User.FirstName!.Contains(term) ||
                        r.User.LastName!.Contains(term)));
            }

            var recordCount = await query.CountAsync();
            var rawItems = await query
                .OrderByDescending(x => x.IssuedAtUtc)
                .ThenByDescending(x => x.WarrantyCardID)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.WarrantyCardID,
                    x.ProductID,
                    x.Product.ProductName,
                    x.Product.ProductPoint,
                    x.SerialNumber,
                    x.ScratchedCode,
                    x.IssuedAtUtc,
                    x.ProductAssignedAtUtc,
                    x.IsRegistered,
                    UserID = x.CardRegistrations
                        .OrderBy(r => r.CreatedAt)
                        .Select(r => r.UserID)
                        .FirstOrDefault(),
                    UserName = x.CardRegistrations
                        .OrderBy(r => r.CreatedAt)
                        .Select(r => ((r.User.FirstName ?? "") + " " + (r.User.LastName ?? "")).Trim())
                        .FirstOrDefault(),
                    PhoneNumber = x.CardRegistrations
                        .OrderBy(r => r.CreatedAt)
                        .Select(r => r.User.PhoneNumber)
                        .FirstOrDefault(),
                    RegisteredAtUtc = x.CardRegistrations
                        .Select(r => (DateTime?)r.CreatedAt)
                        .Min(),
                    StoredPoints = x.CardRegistrations.Sum(r => (int?)r.EarnedPionts) ?? 0,
                    LegacyRegistrationCount = x.CardRegistrations.Count(r => r.EarnedPionts == 0)
                })
                .ToListAsync();

            var items = rawItems.Select(x => new ProductCardDetailItem
            {
                WarrantyCardID = x.WarrantyCardID,
                ProductID = x.ProductID,
                ProductName = x.ProductName,
                SerialNumber = x.SerialNumber,
                ScratchedCode = x.ScratchedCode,
                IssuedAtUtc = x.IssuedAtUtc,
                ProductAssignedAtUtc = x.ProductAssignedAtUtc,
                IsRegistered = x.IsRegistered,
                UserID = x.UserID,
                UserName = x.UserName,
                PhoneNumber = x.PhoneNumber,
                RegisteredAtUtc = x.RegisteredAtUtc,
                AwardedPoints = x.StoredPoints + x.LegacyRegistrationCount * x.ProductPoint
            }).ToList();

            foreach (var item in items.Where(x => string.IsNullOrWhiteSpace(x.UserName)))
                item.UserName = item.PhoneNumber;

            return new ReportPage<ProductCardDetailItem>
            {
                Items = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = recordCount
            };
        }
    }
}
