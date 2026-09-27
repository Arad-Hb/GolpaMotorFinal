using DomainModel.Models;
using DomainModel.ViewModels.Reward;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Queries
{
    public static class RewardRequestQueries
    {
        public static IQueryable<RewardRequest> All(GolpaMotorDbContext db)
            => db.RewardRequests.AsNoTracking();

        public static IQueryable<RewardRequest> ApplySearch(
            IQueryable<RewardRequest> query,
            RewardRequestSearchModel searchModel)
        {
            if (searchModel.RewardDeliveryStatusID.HasValue && searchModel.RewardDeliveryStatusID.Value > 0)
                query = query.Where(x => x.RewardDeliveryStatusID == searchModel.RewardDeliveryStatusID.Value);

            if (searchModel.IsComplete.HasValue)
                query = query.Where(x => x.IsComplete == searchModel.IsComplete.Value);

            if (!string.IsNullOrWhiteSpace(searchModel.SearchTerm))
            {
                var term = searchModel.SearchTerm.Trim();
                query = query.Where(x =>
                    x.RewardCatalog.Title.Contains(term) ||
                    (x.User.FirstName != null && x.User.FirstName.Contains(term)) ||
                    (x.User.LastName != null && x.User.LastName.Contains(term)) ||
                    (x.User.PhoneNumber != null && x.User.PhoneNumber.Contains(term)));
            }

            if (searchModel.RewardCatalogID.HasValue && searchModel.RewardCatalogID.Value > 0)
                query = query.Where(x => x.RewardCatalogID == searchModel.RewardCatalogID.Value);

            if (searchModel.RequestFrom.HasValue)
                query = query.Where(x => x.RequestDate != null && x.RequestDate >= searchModel.RequestFrom.Value);

            if (searchModel.RequestTo.HasValue)
            {
                var to = searchModel.RequestTo.Value.Date.AddDays(1);
                query = query.Where(x => x.RequestDate != null && x.RequestDate < to);
            }

            return query;
        }
    }
}
