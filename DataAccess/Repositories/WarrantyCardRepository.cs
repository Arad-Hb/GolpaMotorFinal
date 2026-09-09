using DataAccess.Services;
using DomainModel.Models;
using Framework.Common;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class WarrantyCardRepository : IWarrantyCardRepository
    {
        private readonly GolpaMotorDbContext db;

        public WarrantyCardRepository(GolpaMotorDbContext db)
        {
            this.db = db;
        }

        public void AddRange(List<WarrantyCard> cards)
        {
            db.WarrantyCards.AddRange(cards);
        }

        public async Task AddAsync(WarrantyCard card)
        {
            await db.WarrantyCards.AddAsync(card);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public Task SaveAsync()
        {
            return db.SaveChangesAsync();
        }

        public async Task<bool> SerialExistsAsync(string serialNumber)
        {
            return await db.WarrantyCards.AnyAsync(x => x.SerialNumber == serialNumber);
        }

        public async Task<HashSet<string>> GetSerialsAsync()
        {
            var list = await db.WarrantyCards.Select(x => x.SerialNumber).ToListAsync();
            return new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<(List<WarrantyCardListItem> Items, int Total)> SearchAsync(WarrantyCardSearchModel search)
        {
            search ??= new WarrantyCardSearchModel();
            var query = db.WarrantyCards.AsQueryable();

            if (search.ProductID.HasValue && search.ProductID.Value > 0)
                query = query.Where(x => x.ProductID == search.ProductID.Value);

            if (search.IsRegistered.HasValue)
                query = query.Where(x => x.IsRegistered == search.IsRegistered.Value);

            if (!string.IsNullOrWhiteSpace(search.SearchTerm))
            {
                var term = search.SearchTerm.Trim();
                query = query.Where(x =>
                    x.SerialNumber.Contains(term) ||
                    x.ScratchedCode.Contains(term) ||
                    x.Product.ProductName.Contains(term) ||
                    x.CardRegistrations.Any(r => r.CustomerPhoneNumber.Contains(term)));
            }

            if (search.RegisteredFrom.HasValue || search.RegisteredTo.HasValue)
            {
                var from = search.RegisteredFrom ?? DateTime.MinValue;
                var to = search.RegisteredTo?.Date.AddDays(1) ?? DateTime.MaxValue;
                query = query.Where(x => x.CardRegistrations.Any(r => r.CreatedAt >= from && r.CreatedAt < to));
            }

            var today = DateTime.Today;
            var filtered = query.Select(x => new
            {
                x.WarrantyCardID,
                x.SerialNumber,
                x.ScratchedCode,
                ProductName = x.Product.ProductName,
                x.IsRegistered,
                x.ValidityMonths,
                RegisteredAt = x.CardRegistrations.Select(r => (DateTime?)r.CreatedAt).Min()
            });

            var needsValidity = !string.IsNullOrWhiteSpace(search.ValidityPreset) ||
                search.RemainingDaysFrom.HasValue ||
                search.RemainingDaysTo.HasValue;

            if (needsValidity)
            {
                filtered = filtered.Where(x => x.IsRegistered && x.RegisteredAt != null);
                if (search.ValidityPreset == "expired")
                    filtered = filtered.Where(x => EF.Functions.DateDiffDay(today, x.RegisteredAt!.Value.AddMonths(x.ValidityMonths)) < 0);
                else if (search.ValidityPreset == "d10")
                    filtered = filtered.Where(x =>
                        EF.Functions.DateDiffDay(today, x.RegisteredAt!.Value.AddMonths(x.ValidityMonths)) >= 0 &&
                        EF.Functions.DateDiffDay(today, x.RegisteredAt!.Value.AddMonths(x.ValidityMonths)) <= 10);
                else if (search.ValidityPreset == "d30")
                    filtered = filtered.Where(x =>
                        EF.Functions.DateDiffDay(today, x.RegisteredAt!.Value.AddMonths(x.ValidityMonths)) >= 0 &&
                        EF.Functions.DateDiffDay(today, x.RegisteredAt!.Value.AddMonths(x.ValidityMonths)) <= 30);

                if (search.RemainingDaysFrom.HasValue)
                {
                    var fromDays = search.RemainingDaysFrom.Value;
                    filtered = filtered.Where(x =>
                        EF.Functions.DateDiffDay(today, x.RegisteredAt!.Value.AddMonths(x.ValidityMonths)) >= fromDays);
                }
                if (search.RemainingDaysTo.HasValue)
                {
                    var toDays = search.RemainingDaysTo.Value;
                    filtered = filtered.Where(x =>
                        EF.Functions.DateDiffDay(today, x.RegisteredAt!.Value.AddMonths(x.ValidityMonths)) <= toDays);
                }
            }

            var pageSize = search.PageSize <= 0 ? 10 : search.PageSize;
            var pageIndex = search.PageIndex < 0 ? 0 : search.PageIndex;

            var total = await filtered.CountAsync();
            var pageCount = pageSize == 0 ? 1 : (int)Math.Ceiling(total / (double)pageSize);
            if (pageCount > 0 && pageIndex >= pageCount)
                pageIndex = pageCount - 1;
            search.PageIndex = pageIndex;

            var rows = await filtered
                .OrderByDescending(x => x.WarrantyCardID)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(x => new WarrantyCardListItem
                {
                    WarrantyCardID = x.WarrantyCardID,
                    SerialNumber = x.SerialNumber,
                    ScratchedCode = x.ScratchedCode,
                    ProductName = x.ProductName,
                    IsRegistered = x.IsRegistered,
                    ValidityMonths = x.ValidityMonths,
                    RegisteredAt = x.RegisteredAt
                })
                .ToListAsync();

            foreach (var item in rows)
            {
                item.RemainingDays = WarrantyValidity.RemainingDays(item.RegisteredAt, item.ValidityMonths, today);
                item.RemainingText = item.IsRegistered
                    ? WarrantyValidity.Format(item.RemainingDays)
                    : "شروع‌نشده";
            }

            return (rows, total);
        }
    }
}
