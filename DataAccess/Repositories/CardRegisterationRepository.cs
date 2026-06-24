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

        public async Task<WarrantyCard?> GetBySerialAsync(string serial, string code)
        {
            return await db.WarrantyCards
                .FirstOrDefaultAsync(x =>
                    x.SerialNumber == serial &&
                    x.ScratchedCode == code);
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

        public Task<bool> IsRegisteredAsync(long cardId)
        {
            throw new NotImplementedException();
        }
    }
}
