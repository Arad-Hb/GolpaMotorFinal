using DomainModel.Models;
using Framework.Common;

namespace DataAccess.Services
{
    public interface ILookupRepository
    {
        Task<List<Province>> GetProvinces();
        Task<List<City>> GetCitiesByProvinceId(int provinceId);
        Task<List<CustomerType>> GetCustomerTypes();
        Task<CustomerType?> GetCustomerType(int id);
        Task<bool> CustomerTypeTitleExists(string title, int? exceptId);
        Task<bool> CustomerTypeInUse(int id);
        Task<OperationResult> AddCustomerType(string title);
        Task<OperationResult> UpdateCustomerType(int id, string title);
        Task<OperationResult> DeleteCustomerType(int id);
    }
}
