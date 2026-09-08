using DataAccess.Services;
using DomainModel.Models;
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

        public async Task<(List<WarrantyCardListItem> Items, int Total)> SearchAsync(long? productId, bool? isRegistered, int pageIndex = 0, int pageSize = 10)
        {
            var query = db.WarrantyCards.Include(x => x.Product).AsQueryable();

            if (productId.HasValue && productId.Value > 0)
                query = query.Where(x => x.ProductID == productId.Value);

            if (isRegistered.HasValue)
                query = query.Where(x => x.IsRegistered == isRegistered.Value);

            if (pageSize <= 0)
                pageSize = 10;
            if (pageIndex < 0)
                pageIndex = 0;

            var total = await query.CountAsync();
            var pageCount = pageSize == 0 ? 1 : (int)Math.Ceiling(total / (double)pageSize);
            if (pageCount > 0 && pageIndex >= pageCount)
                pageIndex = pageCount - 1;

            var items = await query
                .OrderByDescending(x => x.WarrantyCardID)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(x => new WarrantyCardListItem
                {
                    WarrantyCardID = x.WarrantyCardID,
                    SerialNumber = x.SerialNumber,
                    ScratchedCode = x.ScratchedCode,
                    ProductName = x.Product.ProductName,
                    IsRegistered = x.IsRegistered,
                    ValidityMonths = x.ValidityMonths
                })
                .ToListAsync();

            return (items, total);
        }
    }
}
