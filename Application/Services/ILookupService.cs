using DomainModel.Models;
using DomainModel.ViewModels.Settings;
using Framework.Common;

namespace Application.Services
{
    public interface ILookupService
    {
        Task<List<Province>> GetProvinces();
        Task<List<City>> GetCitiesByProvinceId(int provinceId);
        Task<List<CustomerType>> GetCustomerTypes();
        Task<CustomerTypeAddEditModel?> GetCustomerType(int id);
        Task<OperationResult> AddCustomerType(CustomerTypeAddEditModel model);
        Task<OperationResult> UpdateCustomerType(CustomerTypeAddEditModel model);
        Task<OperationResult> DeleteCustomerType(int id);
    }
}
