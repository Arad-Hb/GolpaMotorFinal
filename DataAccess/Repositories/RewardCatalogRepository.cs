using DataAccess.Helpers;
using DataAccess.Mappers;
using DataAccess.Queries;
using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Reward;
using Framework.Common;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class RewardCatalogRepository : IRewardCatalogRepository
    {
        private readonly GolpaMotorDbContext db;

        public RewardCatalogRepository(GolpaMotorDbContext db)
        {
            this.db = db;
        }

        public async Task<OperationResult> Add(RewardCatalogAddEditModel catalog)
        {
            var op = new OperationResult("Add Reward Catalog");
            try
            {
                var entity = RewardCatalogMapper.ToEntity(catalog);
                db.RewardCatalogs.Add(entity);
                await db.SaveChangesAsync();
                await RewardEligibilityHelper.RefreshAllUsersAsync(db);
                return op.ToSuccess("پاداش با موفقیت اضافه شد", entity.RewardCatalogID);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ثبت پاداش: " + ex.Message);
            }
        }

        public async Task<OperationResult> Update(RewardCatalogAddEditModel catalog)
        {
            var op = new OperationResult("Update Reward Catalog");
            if (catalog.RewardCatalogID <= 0)
                return op.ToFailed("شناسه نامعتبر است");

            try
            {
                var entity = await db.RewardCatalogs
                    .FirstOrDefaultAsync(x => x.RewardCatalogID == catalog.RewardCatalogID);

                if (entity == null)
                    return op.ToFailed("پاداش پیدا نشد");

                RewardCatalogMapper.Apply(entity, catalog);

                await db.SaveChangesAsync();
                await RewardEligibilityHelper.RefreshAllUsersAsync(db);
                return op.ToSuccess("پاداش با موفقیت ویرایش شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ویرایش پاداش: " + ex.Message);
            }
        }

        public async Task<OperationResult> Delete(int rewardCatalogID)
        {
            var op = new OperationResult("Delete Reward Catalog");
            try
            {
                var entity = await db.RewardCatalogs
                    .FirstOrDefaultAsync(x => x.RewardCatalogID == rewardCatalogID);

                if (entity == null)
                    return op.ToFailed("پاداش پیدا نشد");

                entity.IsActive = false;
                await db.SaveChangesAsync();
                await RewardEligibilityHelper.RefreshAllUsersAsync(db);
                return op.ToSuccess("پاداش غیرفعال شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف پاداش: " + ex.Message);
            }
        }

        public async Task<RewardCatalogAddEditModel?> Get(int rewardCatalogID)
        {
            var entity = await RewardCatalogQueries.All(db)
                .FirstOrDefaultAsync(x => x.RewardCatalogID == rewardCatalogID);

            return entity == null ? null : RewardCatalogMapper.ToAddEditModel(entity);
        }

        public async Task<List<RewardCatalogListItem>> GetAll()
        {
            return await RewardCatalogQueries.All(db)
                .OrderBy(x => x.RequiredPoints)
                .Select(RewardCatalogMapper.ToListItem)
                .ToListAsync();
        }

        public async Task<List<RewardCatalogListItem>> GetActiveCatalogs()
        {
            return await RewardCatalogQueries.Active(db)
                .OrderBy(x => x.RequiredPoints)
                .Select(RewardCatalogMapper.ToListItem)
                .ToListAsync();
        }

        public async Task<RewardCatalogDetailsModel?> GetDetails(int rewardCatalogID)
        {
            return await RewardCatalogQueries.All(db)
                .Where(x => x.RewardCatalogID == rewardCatalogID)
                .Select(RewardCatalogMapper.ToDetails)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> Exists(int rewardCatalogID)
        {
            return await RewardCatalogQueries.All(db).AnyAsync(x => x.RewardCatalogID == rewardCatalogID);
        }

        public async Task<RewardCatalogListComplexModel> Search(RewardCatalogSearchModel searchModel)
        {
            var result = new RewardCatalogListComplexModel();
            var query = RewardCatalogQueries.ApplySearch(RewardCatalogQueries.All(db), searchModel);

            var totalCount = await query.CountAsync();
            var pageIndex = searchModel.PageIndex < 0 ? 0 : searchModel.PageIndex;
            var pageSize = searchModel.PageSize <= 0 ? 10 : searchModel.PageSize;

            var list = await query
                .OrderBy(x => x.RequiredPoints)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(RewardCatalogMapper.ToListItem)
                .ToListAsync();

            result.sm = searchModel;
            result.sm.RecordCount = totalCount;
            result.CatalogList = list;
            return result;
        }
    }
}
