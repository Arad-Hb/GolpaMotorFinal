using DataAccess.Helpers;
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

        private static RewardCatalog ToDbModel(RewardCatalogAddEditModel model)
        {
            return new RewardCatalog
            {
                Title = model.Title,
                Description = model.Description,
                RequiredPoints = model.RequiredPoints,
                IsCashReward = model.IsCashReward,
                CashValue = model.IsCashReward ? model.CashValue : null,
                IsActive = model.IsActive
            };
        }

        private static RewardCatalogAddEditModel ToViewModel(RewardCatalog catalog)
        {
            return new RewardCatalogAddEditModel
            {
                RewardCatalogID = catalog.RewardCatalogID,
                Title = catalog.Title,
                Description = catalog.Description,
                RequiredPoints = catalog.RequiredPoints,
                IsCashReward = catalog.IsCashReward,
                CashValue = catalog.CashValue,
                IsActive = catalog.IsActive
            };
        }

        public async Task<OperationResult> Add(RewardCatalogAddEditModel catalog)
        {
            var op = new OperationResult("Add Reward Catalog");
            try
            {
                var entity = ToDbModel(catalog);
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

                entity.Title = catalog.Title;
                entity.Description = catalog.Description;
                entity.RequiredPoints = catalog.RequiredPoints;
                entity.IsCashReward = catalog.IsCashReward;
                entity.CashValue = catalog.IsCashReward ? catalog.CashValue : null;
                entity.IsActive = catalog.IsActive;

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
            var entity = await db.RewardCatalogs
                .FirstOrDefaultAsync(x => x.RewardCatalogID == rewardCatalogID);

            return entity == null ? null : ToViewModel(entity);
        }

        public async Task<List<RewardCatalogListItem>> GetAll()
        {
            return await db.RewardCatalogs
                .OrderBy(x => x.RequiredPoints)
                .Select(x => new RewardCatalogListItem
                {
                    RewardCatalogID = x.RewardCatalogID,
                    Title = x.Title,
                    Description = x.Description,
                    RequiredPoints = x.RequiredPoints,
                    IsCashReward = x.IsCashReward,
                    CashValue = x.CashValue,
                    IsActive = x.IsActive,
                    RequestCount = x.RewardRequests.Count
                })
                .ToListAsync();
        }

        public async Task<List<RewardCatalogListItem>> GetActiveCatalogs()
        {
            return await db.RewardCatalogs
                .Where(x => x.IsActive)
                .OrderBy(x => x.RequiredPoints)
                .Select(x => new RewardCatalogListItem
                {
                    RewardCatalogID = x.RewardCatalogID,
                    Title = x.Title,
                    Description = x.Description,
                    RequiredPoints = x.RequiredPoints,
                    IsCashReward = x.IsCashReward,
                    CashValue = x.CashValue,
                    IsActive = x.IsActive,
                    RequestCount = x.RewardRequests.Count
                })
                .ToListAsync();
        }

        public async Task<RewardCatalogDetailsModel?> GetDetails(int rewardCatalogID)
        {
            return await db.RewardCatalogs
                .Where(x => x.RewardCatalogID == rewardCatalogID)
                .Select(x => new RewardCatalogDetailsModel
                {
                    RewardCatalogID = x.RewardCatalogID,
                    Title = x.Title,
                    Description = x.Description,
                    RequiredPoints = x.RequiredPoints,
                    IsCashReward = x.IsCashReward,
                    CashValue = x.CashValue,
                    IsActive = x.IsActive,
                    RequestCount = x.RewardRequests.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> Exists(int rewardCatalogID)
        {
            return await db.RewardCatalogs.AnyAsync(x => x.RewardCatalogID == rewardCatalogID);
        }

        public async Task<RewardCatalogListComplexModel> Search(RewardCatalogSearchModel searchModel)
        {
            var result = new RewardCatalogListComplexModel();
            var query = db.RewardCatalogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchModel.Title))
                query = query.Where(x => x.Title.Contains(searchModel.Title));

            if (searchModel.IsActive.HasValue)
                query = query.Where(x => x.IsActive == searchModel.IsActive.Value);

            if (searchModel.IsCashReward.HasValue)
                query = query.Where(x => x.IsCashReward == searchModel.IsCashReward.Value);

            if (searchModel.RequiredFrom.HasValue)
                query = query.Where(x => x.RequiredPoints >= searchModel.RequiredFrom.Value);

            if (searchModel.RequiredTo.HasValue)
                query = query.Where(x => x.RequiredPoints <= searchModel.RequiredTo.Value);

            var totalCount = await query.CountAsync();
            var pageIndex = searchModel.PageIndex < 0 ? 0 : searchModel.PageIndex;
            var pageSize = searchModel.PageSize <= 0 ? 10 : searchModel.PageSize;

            var list = await query
                .OrderBy(x => x.RequiredPoints)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(x => new RewardCatalogListItem
                {
                    RewardCatalogID = x.RewardCatalogID,
                    Title = x.Title,
                    Description = x.Description,
                    RequiredPoints = x.RequiredPoints,
                    IsCashReward = x.IsCashReward,
                    CashValue = x.CashValue,
                    IsActive = x.IsActive,
                    RequestCount = x.RewardRequests.Count
                })
                .ToListAsync();

            result.sm = searchModel;
            result.sm.RecordCount = totalCount;
            result.CatalogList = list;
            return result;
        }
    }
}
