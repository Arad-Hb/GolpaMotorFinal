using DomainModel.Models;
using DomainModel.ViewModels.Reports;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Queries
{
    public static class ProductWarrantyReportQueries
    {
        public static IQueryable<Product> ApplyProductFilters(IQueryable<Product> products,ProductWarrantyReportSearchModel search)
        {
            if (search.ProductID.HasValue && search.ProductID.Value > 0)
            {
                products = products.Where(x => x.ProductID == search.ProductID.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.SearchTerm))
            {
                var term = search.SearchTerm.Trim();
                products = products.Where(x => x.ProductName.Contains(term));
            }

            return products;
        }

        public static IQueryable<WarrantyCard> ApplyCardFilters(IQueryable<WarrantyCard> cards,ProductWarrantyReportSearchModel search)
        {
            if (search.FromUtc.HasValue)
            {
                cards = cards.Where(x => x.IssuedAtUtc >= search.FromUtc.Value);
            }

            if (search.ToUtcExclusive.HasValue)
            {
                cards = cards.Where(x => x.IssuedAtUtc < search.ToUtcExclusive.Value);
            }

            if (search.IsRegistered.HasValue)
            {
                cards = cards.Where(x => x.IsRegistered == search.IsRegistered.Value);
            }

            return cards;
        }

        public static IQueryable<CardRegistration> ApplyRegistrationDateFilters(IQueryable<CardRegistration> registrations,ProductWarrantyReportSearchModel search)
        {
            if (search.FromUtc.HasValue)
            {
                registrations = registrations.Where(x => x.CreatedAt >= search.FromUtc.Value);
            }

            if (search.ToUtcExclusive.HasValue)
            {
                registrations = registrations.Where(x => x.CreatedAt < search.ToUtcExclusive.Value);
            }

            return registrations;
        }

        public static IQueryable<ProductWarrantyReportRow> BuildSummaryQuery(
            GolpaMotorDbContext db,
            IQueryable<Product> products,
            IQueryable<WarrantyCard> cards,
            IQueryable<CardRegistration> registrations)
        {
            return products.Select(p => new ProductWarrantyReportRow
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
                RegistrantSettledPoints = db.ReportActivityLogs
                    .Where(l =>
                        l.ActivityType == ReportActivityTypes.RewardApproved &&
                        l.RewardRequestID != null &&
                        registrations.Any(r =>
                            r.WarrantyCard.ProductID == p.ProductID &&
                            r.UserID == l.UserID))
                    .GroupBy(l => l.RewardRequestID)
                    .Select(g => g.Max(l => l.PointsDelta < 0 ? -l.PointsDelta : 0))
                    .Sum(),
                RegistrantRemainedPoints =
                    (registrations
                        .Where(r => r.WarrantyCard.ProductID == p.ProductID)
                        .Sum(r => (int?)(r.EarnedPionts != 0
                            ? r.EarnedPionts
                            : r.WarrantyCard.Product.ProductPoint)) ?? 0)
                    - db.ReportActivityLogs
                        .Where(l =>
                            l.ActivityType == ReportActivityTypes.RewardApproved &&
                            l.RewardRequestID != null &&
                            registrations.Any(r =>
                                r.WarrantyCard.ProductID == p.ProductID &&
                                r.UserID == l.UserID))
                        .GroupBy(l => l.RewardRequestID)
                        .Select(g => g.Max(l => l.PointsDelta < 0 ? -l.PointsDelta : 0))
                        .Sum()
            });
        }

        public static IQueryable<ProductWarrantyReportRow> ApplyAggregateFilters(IQueryable<ProductWarrantyReportRow> query,ProductWarrantyReportSearchModel search)
        {
            if (search.CountFrom.HasValue)
            {
                query = query.Where(x => x.TotalCards >= search.CountFrom.Value);
            }

            if (search.CountTo.HasValue)
            {
                query = query.Where(x => x.TotalCards <= search.CountTo.Value);
            }

            if (search.PointsFrom.HasValue)
            {
                query = query.Where(x => x.AwardedPoints >= search.PointsFrom.Value);
            }

            if (search.PointsTo.HasValue)
            {
                query = query.Where(x => x.AwardedPoints <= search.PointsTo.Value);
            }

            return query;
        }
    }
}
