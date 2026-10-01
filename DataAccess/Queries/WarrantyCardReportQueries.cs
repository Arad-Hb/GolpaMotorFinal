using DomainModel.Models;
using DomainModel.ViewModels.Reports;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Queries
{
    public static class WarrantyCardReportQueries
    {
        public static IQueryable<WarrantyCard> ApplyCardDetailSearch(
            IQueryable<WarrantyCard> query,
            ReportActivitySearchModel search,
            string? scopedUserId)
        {
            if (search.IsRegistered.HasValue)
            {
                query = query.Where(card => card.IsRegistered == search.IsRegistered.Value);
            }

            if (search.ProductID.HasValue && search.ProductID.Value > 0)
            {
                query = query.Where(x => x.ProductID == search.ProductID.Value);
            }

            if (search.WarrantyCardID.HasValue && search.WarrantyCardID.Value > 0)
            {
                query = query.Where(x => x.WarrantyCardID == search.WarrantyCardID.Value);
            }

            if (search.FromUtc.HasValue)
            {
                query = query.Where(x => x.IssuedAtUtc >= search.FromUtc.Value);
            }

            if (search.ToUtcExclusive.HasValue)
            {
                query = query.Where(x => x.IssuedAtUtc < search.ToUtcExclusive.Value);
            }

            if (scopedUserId != null)
            {
                query = query.Where(x => x.CardRegistrations.Any(r => r.UserID == scopedUserId));
            }

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
                        r.User.LastName!.Contains(term) ||
                        ((r.User.FirstName ?? "") + " " + (r.User.LastName ?? "")).Contains(term)));
            }

            return query;
        }
    }
}
