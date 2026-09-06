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

        public async Task<List<WarrantyCardListItem>> SearchAsync(long? productId, bool? isRegistered)
        {
            var query = db.WarrantyCards.Include(x => x.Product).AsQueryable();

            if (productId.HasValue && productId.Value > 0)
                query = query.Where(x => x.ProductID == productId.Value);

            if (isRegistered.HasValue)
                query = query.Where(x => x.IsRegistered == isRegistered.Value);

            return await query
                .OrderByDescending(x => x.WarrantyCardID)
                .Take(200)
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
        }
    }
}
