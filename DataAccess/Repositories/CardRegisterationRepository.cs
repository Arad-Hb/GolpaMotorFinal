using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class CardRegistrationRepository : ICardRegistrationRepository
    {
        private readonly GolpaMotorDbContext db;

        public CardRegistrationRepository(GolpaMotorDbContext db)
        {
            this.db = db;
        }

        public async Task<(WarrantyCard? Card, bool IsAmbiguous)> GetByScratchedCodeAsync(string code)
        {
            var matches = await db.WarrantyCards
                .Include(x => x.Product)
                .Where(x => x.ScratchedCode == code)
                .Take(2)
                .ToListAsync();

            if (matches.Count > 1)
                return (null, true);

            return (matches.Count == 1 ? matches[0] : null, false);
        }

        public async Task<bool> IsCardAlreadyRegisteredByUserAsync(int CustomerTypeId, string userId)
        {
            return await db.UserCustomerTypes
                .AnyAsync(x => x.CustomerTypeID == CustomerTypeId && x.UserID == userId);
        }

        public async Task AddRegistration(CardRegistration entity)
        {
            await db.CardRegistrations.AddAsync(entity);
        }

        public async Task AddTransaction(PointTransaction entity)
        {
            await db.PointTransactions.AddAsync(entity);
        }

        public async Task<long> GetTotalPoints(string userId)
        {
            return await db.PointTransactions
                .Where(x => x.UserID == userId)
                .SumAsync(x => (long)x.PointsAmount);
        }
        public async Task AddUserCustomerType(UserCustomerType entity)
        {
            await db.UserCustomerTypes.AddAsync(entity);
        }
        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<CustomerType>> GetCustomerTypes()
        {
            return await db.CustomerTypes.Select(c => new CustomerType
            {
                    CustomerTypeID = c.CustomerTypeID,
                    Title = c.Title
            }).ToListAsync();
        }

        public async Task<bool> IsRegisteredAsync(long cardId)
        {
            return await db.CardRegistrations
                .AnyAsync(x => x.WarrantyCardID == cardId)
                || await db.WarrantyCards
                    .AnyAsync(x => x.WarrantyCardID == cardId && x.IsRegistered);
        }

        public async Task<List<CardRegistration>> GetByUserAsync(string userId)
        {
            return await db.CardRegistrations
                .Include(x => x.WarrantyCard)
                    .ThenInclude(x => x.Product)
                .Where(x => x.UserID == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
