using Application.Services;
using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Settings;
using Framework.Common;

namespace ApplicationService.Services
{
    public class LookupService : ILookupService
    {
        private readonly ILookupRepository repo;

        public LookupService(ILookupRepository repo)
        {
            this.repo = repo;
        }

        public Task<List<Province>> GetProvinces()
            => repo.GetProvinces();

        public Task<List<City>> GetCitiesByProvinceId(int provinceId)
            => repo.GetCitiesByProvinceId(provinceId);

        public Task<List<CustomerType>> GetCustomerTypes()
            => repo.GetCustomerTypes();

        public async Task<CustomerTypeAddEditModel?> GetCustomerType(int id)
        {
            var entity = await repo.GetCustomerType(id);
            if (entity == null)
                return null;

            return new CustomerTypeAddEditModel
            {
                CustomerTypeID = entity.CustomerTypeID,
                Title = entity.Title
            };
        }

        public async Task<OperationResult> AddCustomerType(CustomerTypeAddEditModel model)
        {
            var op = new OperationResult("AddCustomerType");
            var title = Normalize(model.Title);
            var invalid = Validate(title);
            if (invalid != null)
                return op.ToFailed(invalid);
            if (await repo.CustomerTypeTitleExists(title, null))
                return op.ToFailed("این عنوان قبلاً ثبت شده است");

            return await repo.AddCustomerType(title);
        }

        public async Task<OperationResult> UpdateCustomerType(CustomerTypeAddEditModel model)
        {
            var op = new OperationResult("UpdateCustomerType");
            if (model.CustomerTypeID <= 0)
                return op.ToFailed("شناسه نامعتبر است");

            var title = Normalize(model.Title);
            var invalid = Validate(title);
            if (invalid != null)
                return op.ToFailed(invalid);
            if (await repo.CustomerTypeTitleExists(title, model.CustomerTypeID))
                return op.ToFailed("این عنوان قبلاً ثبت شده است");

            return await repo.UpdateCustomerType(model.CustomerTypeID, title);
        }

        public async Task<OperationResult> DeleteCustomerType(int id)
        {
            var op = new OperationResult("DeleteCustomerType");
            if (id <= 0)
                return op.ToFailed("شناسه نامعتبر است");
            if (await repo.CustomerTypeInUse(id))
                return op.ToFailed("این نوع مشتری به کاربر اختصاص داده شده و قابل حذف نیست");

            return await repo.DeleteCustomerType(id);
        }

        private static string Normalize(string? title) => (title ?? string.Empty).Trim();

        private static string? Validate(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "عنوان نوع مشتری الزامی است";
            if (title.Length > 50)
                return "عنوان حداکثر ۵۰ کاراکتر است";
            return null;
        }
    }
}
