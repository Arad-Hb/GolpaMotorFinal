using DomainModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Services
{
    public interface ICardRegistrationRepository
    {
        Task<WarrantyCard?> GetBySerialAsync(string serial, string code);
        Task<bool> IsRegisteredAsync(long cardId);
        Task AddRegistration(CardRegistration entity);
        Task AddTransaction(PointTransaction entity);
        Task<long> GetTotalPoints(string userId);
        Task AddUserCustomerType(UserCustomerType entity);
        Task<IEnumerable<CustomerType>> GetCustomerTypes();
        Task<bool> IsCardAlreadyRegisteredByUserAsync(int CustomerTypeId, string userId);
        Task SaveChangesAsync();
    }
}
