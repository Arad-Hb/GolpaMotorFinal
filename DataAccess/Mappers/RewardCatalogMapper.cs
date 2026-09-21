using DomainModel.Models;
using DomainModel.ViewModels.Reward;
using System.Linq.Expressions;

namespace DataAccess.Mappers
{
    public static class RewardCatalogMapper
    {
        public static RewardCatalog ToEntity(RewardCatalogAddEditModel model)
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

        public static void Apply(RewardCatalog entity, RewardCatalogAddEditModel model)
        {
            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.RequiredPoints = model.RequiredPoints;
            entity.IsCashReward = model.IsCashReward;
            entity.CashValue = model.IsCashReward ? model.CashValue : null;
            entity.IsActive = model.IsActive;
        }

        public static RewardCatalogAddEditModel ToAddEditModel(RewardCatalog catalog)
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

        public static Expression<Func<RewardCatalog, RewardCatalogListItem>> ToListItem =>
            x => new RewardCatalogListItem
            {
                RewardCatalogID = x.RewardCatalogID,
                Title = x.Title,
                Description = x.Description,
                RequiredPoints = x.RequiredPoints,
                IsCashReward = x.IsCashReward,
                CashValue = x.CashValue,
                IsActive = x.IsActive,
                RequestCount = x.RewardRequests.Count
            };

        public static Expression<Func<RewardCatalog, RewardCatalogDetailsModel>> ToDetails =>
            x => new RewardCatalogDetailsModel
            {
                RewardCatalogID = x.RewardCatalogID,
                Title = x.Title,
                Description = x.Description,
                RequiredPoints = x.RequiredPoints,
                IsCashReward = x.IsCashReward,
                CashValue = x.CashValue,
                IsActive = x.IsActive,
                RequestCount = x.RewardRequests.Count
            };
    }
}
