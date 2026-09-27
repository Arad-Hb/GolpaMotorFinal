using DomainModel.Models;
using DomainModel.ViewModels.Reward;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Queries
{
    public static class RewardCatalogQueries
    {
        public static IQueryable<RewardCatalog> All(GolpaMotorDbContext db)
            => db.RewardCatalogs.AsNoTracking();

        public static IQueryable<RewardCatalog> Active(GolpaMotorDbContext db)
            => All(db).Where(x => x.IsActive);

        public static IQueryable<RewardCatalog> ApplySearch(
            IQueryable<RewardCatalog> query,
            RewardCatalogSearchModel searchModel)
        {
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

            return query;
        }
    }
}
