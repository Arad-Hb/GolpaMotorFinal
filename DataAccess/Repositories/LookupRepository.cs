using DataAccess.Services;
using DomainModel.Models;
using Framework.Common;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class LookupRepository : ILookupRepository
    {
        private readonly GolpaMotorDbContext db;

        public LookupRepository(GolpaMotorDbContext db)
        {
            this.db = db;
        }

        public Task<List<Province>> GetProvinces()
            => db.Provinces.OrderBy(x => x.Name).ToListAsync();

        public Task<List<City>> GetCitiesByProvinceId(int provinceId)
            => db.Cities.Where(c => c.ProvinceID == provinceId).OrderBy(c => c.Name).ToListAsync();

        public Task<List<CustomerType>> GetCustomerTypes()
            => db.CustomerTypes.OrderBy(x => x.Title).ToListAsync();

        public Task<CustomerType?> GetCustomerType(int id)
            => db.CustomerTypes.FirstOrDefaultAsync(x => x.CustomerTypeID == id);

        public Task<bool> CustomerTypeTitleExists(string title, int? exceptId)
            => db.CustomerTypes.AnyAsync(x => x.Title == title && (exceptId == null || x.CustomerTypeID != exceptId));

        public Task<bool> CustomerTypeInUse(int id)
            => db.UserCustomerTypes.AnyAsync(x => x.CustomerTypeID == id);

        public async Task<OperationResult> AddCustomerType(string title)
        {
            var op = new OperationResult("AddCustomerType");
            var entity = new CustomerType { Title = title };
            db.CustomerTypes.Add(entity);
            await db.SaveChangesAsync();
            return op.ToSuccess("نوع مشتری با موفقیت افزوده شد", entity.CustomerTypeID);
        }

        public async Task<OperationResult> UpdateCustomerType(int id, string title)
        {
            var op = new OperationResult("UpdateCustomerType");
            var entity = await db.CustomerTypes.FirstOrDefaultAsync(x => x.CustomerTypeID == id);
            if (entity == null)
                return op.ToFailed("نوع مشتری پیدا نشد");

            entity.Title = title;
            await db.SaveChangesAsync();
            return op.ToSuccess("نوع مشتری با موفقیت ویرایش شد");
        }

        public async Task<OperationResult> DeleteCustomerType(int id)
        {
            var op = new OperationResult("DeleteCustomerType");
            var entity = await db.CustomerTypes.FirstOrDefaultAsync(x => x.CustomerTypeID == id);
            if (entity == null)
                return op.ToFailed("نوع مشتری پیدا نشد");

            db.CustomerTypes.Remove(entity);
            await db.SaveChangesAsync();
            return op.ToSuccess("نوع مشتری با موفقیت حذف شد");
        }
    }
}
