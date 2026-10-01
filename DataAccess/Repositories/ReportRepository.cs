using DataAccess.Mappers;
using DataAccess.Queries;
using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Product;
using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.Reward;
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
            var query = WarrantyCardQueries.All(db);
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
            if (!jalaliYear.HasValue && !jalaliMonth.HasValue)
            {
                return await db.CardRegistrations
                    .AsNoTracking()
                    .GroupBy(x => x.WarrantyCard.Product.ProductName)
                    .Select(g => new ProductPopularityRow
                    {
                        ProductName = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.ProductName)
                    .ToListAsync();
            }

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

        public Task<List<RewardPopularityRow>> GetRewardPopularity(DateTime? from, DateTime? to) =>
            RewardReportQueries.LoadPopularityRows(db, from, to);

        public async Task<ReportPage<WarrantyProductStatusRow>> SearchWarrantyByProduct(
            long? productId,
            DateTime? from,
            DateTime? to,
            int pageIndex,
            int pageSize)
        {
            var rows = await GetWarrantyByProduct(productId, from, to);
            return ToPage(rows, pageIndex, pageSize);
        }

        public async Task<ReportPage<ProductPopularityRow>> SearchProductPopularity(
            int? jalaliYear,
            int? jalaliMonth,
            int pageIndex,
            int pageSize)
        {
            var rows = await GetProductPopularity(jalaliYear, jalaliMonth);
            return ToPage(rows, pageIndex, pageSize);
        }

        public async Task<ReportPage<RewardPopularityRow>> SearchRewardPopularity(
            DateTime? from,
            DateTime? to,
            int pageIndex,
            int pageSize)
        {
            var rows = await GetRewardPopularity(from, to);
            return ToPage(rows, pageIndex, pageSize);
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
            return await RewardRequestQueries.All(db)
                .GroupBy(x => x.RewardCatalog.Title)
                .Select(g => new NamedCountItem { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(take)
                .ToListAsync();
        }

        public async Task<AdminDashboardSummary> GetAdminDashboardSummary()
        {
            var users = UserQueries.Active(db);
            var rewardRequests = RewardRequestQueries.All(db);

            return new AdminDashboardSummary
            {
                TotalEarnedPoints = await users.SumAsync(x => (int?)x.TotalEarnedPoints) ?? 0,
                TotalSettledPoints = await users.SumAsync(x => (int?)x.TotalSettledPoints) ?? 0,
                TotalRemainedPoints = await users.SumAsync(x => (int?)x.RemainedPoints) ?? 0,
                TotalRewardRequests = await rewardRequests.CountAsync(),
                SettledRewardRequests = await rewardRequests.CountAsync(x =>
                    x.IsComplete ||
                    x.RewardDeliveryStatus.Title == RewardStatusTitles.Approved ||
                    x.RewardDeliveryStatus.Title == RewardStatusTitles.Paid),
                PendingRewardRequests = await rewardRequests.CountAsync(x =>
                    !x.IsComplete &&
                    x.RewardDeliveryStatus.Title == RewardStatusTitles.Pending)
            };
        }

        public async Task<List<RewardRequestListItem>> GetRecentPendingRewardRequests(int take = 8)
        {
            if (take <= 0)
                take = 8;

            return await RewardRequestMapper.ToListItems(
                    RewardRequestQueries.All(db)
                        .Where(x =>
                            !x.IsComplete &&
                            x.RewardDeliveryStatus.Title == RewardStatusTitles.Pending))
                .OrderByDescending(x => x.RequestDate)
                .ThenByDescending(x => x.RewardRequestID)
                .Take(take)
                .ToListAsync();
        }

        public async Task<ReportPage<DashboardRegistrarItem>> GetDashboardRegistrars(int pageIndex,int pageSize)
        {
            pageIndex = Math.Max(0, pageIndex);
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = UserQueries.Registrars(db);

            var recordCount = await query.CountAsync();
            var pageCount = (int)Math.Ceiling(recordCount / (double)pageSize);
            if (pageCount > 0 && pageIndex >= pageCount)
                pageIndex = pageCount - 1;

            var items = await query
                .OrderByDescending(x => x.RegistrationCount)
                .ThenBy(x => x.FullName)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach (var item in items.Where(x => string.IsNullOrWhiteSpace(x.FullName)))
                item.FullName = item.PhoneNumber ?? "نامشخص";

            return new ReportPage<DashboardRegistrarItem>
            {
                Items = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = recordCount
            };
        }

        public async Task<List<DashboardWarrantyAlertItem>> GetWarrantyExpiryAlerts(int take = 10,int withinDays = 30)
        {
            take = take <= 0 ? 10 : take;
            withinDays = Math.Max(0, withinDays);
            var today = DateTime.UtcNow.ToIranTime().Date;

            var candidates = WarrantyCardQueries.All(db)
                .Where(x => x.IsRegistered && x.CardRegistrations.Any())
                .Select(x => new
                {
                    x.WarrantyCardID,
                    x.SerialNumber,
                    x.ScratchedCode,
                    x.ValidityMonths,
                    ProductName = x.Product.ProductName,
                    RegisteredAt = x.CardRegistrations.Select(r => (DateTime?)r.CreatedAt).Min(),
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
                        .Select(r => r.User.PhoneNumber ?? r.CustomerPhoneNumber)
                        .FirstOrDefault()
                });

            var rows = db.Database.IsSqlServer()
                ? await candidates
                    .Where(x => x.RegisteredAt.HasValue &&
                        EF.Functions.DateDiffDay(
                            today,
                            x.RegisteredAt.Value.AddMonths(x.ValidityMonths)) <= withinDays)
                    .OrderBy(x => EF.Functions.DateDiffDay(
                        today,
                        x.RegisteredAt!.Value.AddMonths(x.ValidityMonths)))
                    .ThenBy(x => x.WarrantyCardID)
                    .Take(take)
                    .ToListAsync()
                : (await candidates.ToListAsync())
                    .Where(x => x.RegisteredAt.HasValue &&
                        WarrantyValidity.RemainingDays(
                            x.RegisteredAt.Value.ToIranTime(),
                            x.ValidityMonths,
                            today) <= withinDays)
                    .OrderBy(x => WarrantyValidity.RemainingDays(
                        x.RegisteredAt!.Value.ToIranTime(),
                        x.ValidityMonths,
                        today))
                    .ThenBy(x => x.WarrantyCardID)
                    .Take(take)
                    .ToList();

            return rows.Select(x =>
            {
                var registeredAt = x.RegisteredAt!.Value;
                var localRegisteredAt = registeredAt.ToIranTime();
                var remainingDays = WarrantyValidity.RemainingDays(
                    localRegisteredAt,
                    x.ValidityMonths,
                    today) ?? 0;
                return new DashboardWarrantyAlertItem
                {
                    WarrantyCardID = x.WarrantyCardID,
                    UserID = x.UserID ?? string.Empty,
                    UserName = string.IsNullOrWhiteSpace(x.UserName)
                        ? x.PhoneNumber ?? "نامشخص"
                        : x.UserName,
                    PhoneNumber = x.PhoneNumber,
                    ProductName = x.ProductName,
                    SerialNumber = x.SerialNumber,
                    ScratchedCode = x.ScratchedCode,
                    RegisteredAtUtc = registeredAt,
                    ExpiresAt = localRegisteredAt.Date.AddMonths(x.ValidityMonths),
                    RemainingDays = remainingDays,
                    RemainingText = WarrantyValidity.Format(remainingDays)
                };
            }).ToList();
        }

        public async Task<ReportPage<ProductWarrantyReportRow>> SearchProductWarranty(ProductWarrantyReportSearchModel search)
        {
            search ??= new ProductWarrantyReportSearchModel();
            var pageIndex = Math.Max(0, search.PageIndex);
            var pageSize = search.PageSize <= 0 ? 50 : search.PageSize;

            var products = ProductWarrantyReportQueries.ApplyProductFilters(ProductQueries.Active(db), search);
            var cards = ProductWarrantyReportQueries.ApplyCardFilters(WarrantyCardQueries.All(db), search);
            var registrations = ProductWarrantyReportQueries.ApplyRegistrationDateFilters(
                db.CardRegistrations.AsNoTracking().AsQueryable(),
                search);

            var query = ProductWarrantyReportQueries.ApplyAggregateFilters(
                ProductWarrantyReportQueries.BuildSummaryQuery(db, products, cards, registrations),
                search);

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

        public async Task<ReportPage<ReportActivityItem>> SearchActivities(ReportActivitySearchModel search)
        {
            search ??= new ReportActivitySearchModel();
            var pageIndex = Math.Max(0, search.PageIndex);
            var pageSize = search.PageSize <= 0 ? 50 : search.PageSize;
            var query = ReportActivityQueries.ApplySearch(ReportActivityQueries.Base(db), search);

            var recordCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.OccurredAtUtc)
                .ThenByDescending(x => x.ReportActivityLogID)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(ReportMapper.ToActivityItem)
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

        public async Task<ReportPage<ProductCardDetailItem>> SearchProductCards(ReportActivitySearchModel search)
        {
            search ??= new ReportActivitySearchModel();
            var pageIndex = Math.Max(0, search.PageIndex);
            var pageSize = search.PageSize <= 0 ? 50 : search.PageSize;
            var scopedUserId = string.IsNullOrWhiteSpace(search.UserID) ? null : search.UserID;
            var query = WarrantyCardReportQueries.ApplyCardDetailSearch(
                WarrantyCardQueries.All(db),
                search,
                scopedUserId);

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
                    x.ValidityMonths,
                    UserID = x.CardRegistrations
                        .Where(r => scopedUserId == null || r.UserID == scopedUserId)
                        .OrderBy(r => r.CreatedAt)
                        .Select(r => r.UserID)
                        .FirstOrDefault(),
                    UserName = x.CardRegistrations
                        .Where(r => scopedUserId == null || r.UserID == scopedUserId)
                        .OrderBy(r => r.CreatedAt)
                        .Select(r => ((r.User.FirstName ?? "") + " " + (r.User.LastName ?? "")).Trim())
                        .FirstOrDefault(),
                    PhoneNumber = x.CardRegistrations
                        .Where(r => scopedUserId == null || r.UserID == scopedUserId)
                        .OrderBy(r => r.CreatedAt)
                        .Select(r => r.User.PhoneNumber)
                        .FirstOrDefault(),
                    RegisteredAtUtc = x.CardRegistrations
                        .Where(r => scopedUserId == null || r.UserID == scopedUserId)
                        .Select(r => (DateTime?)r.CreatedAt)
                        .Min(),
                    StoredPoints = x.CardRegistrations
                        .Where(r => scopedUserId == null || r.UserID == scopedUserId)
                        .Sum(r => (int?)r.EarnedPionts) ?? 0,
                    LegacyRegistrationCount = x.CardRegistrations.Count(r =>
                        (scopedUserId == null || r.UserID == scopedUserId) &&
                        r.EarnedPionts == 0)
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
                AwardedPoints = x.StoredPoints + x.LegacyRegistrationCount * x.ProductPoint,
                PointsAwardedAtUtc = x.RegisteredAtUtc,
                ValidityMonths = x.ValidityMonths
            }).ToList();

            var today = DateTime.UtcNow.ToIranTime().Date;
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.UserName))
                    item.UserName = item.PhoneNumber;

                item.RemainingDays = WarrantyValidity.RemainingDays(
                    item.RegisteredAtUtc?.ToIranTime(),
                    item.ValidityMonths,
                    today);
                item.RemainingValidityText = item.IsRegistered
                    ? WarrantyValidity.Format(item.RemainingDays)
                    : "شروع‌نشده";
            }

            return new ReportPage<ProductCardDetailItem>
            {
                Items = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = recordCount
            };
        }

        private static ReportPage<T> ToPage<T>(List<T> rows, int pageIndex, int pageSize)
        {
            pageSize = pageSize <= 0 ? 50 : pageSize;
            pageIndex = Math.Max(0, pageIndex);

            var pageCount = (int)Math.Ceiling(rows.Count / (double)pageSize);
            var lastPageIndex = Math.Max(0, pageCount - 1);
            pageIndex = Math.Min(pageIndex, lastPageIndex);

            return new ReportPage<T>
            {
                Items = rows.Skip(pageIndex * pageSize).Take(pageSize).ToList(),
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = rows.Count
            };
        }
    }
}
