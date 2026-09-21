using DomainModel.Models;
using DomainModel.ViewModels.Reward;

namespace DataAccess.Mappers
{
    public static class RewardRequestMapper
    {
        public static IQueryable<RewardRequestListItem> ToListItems(IQueryable<RewardRequest> query)
        {
            return query.Select(x => new RewardRequestListItem
            {
                RewardRequestID = x.RewardRequestID,
                UserID = x.UserID,
                UserFullName = ((x.User.FirstName ?? "") + " " + (x.User.LastName ?? "")),
                PhoneNumber = x.User.PhoneNumber,
                RewardCatalogID = x.RewardCatalogID,
                CatalogTitle = x.RewardCatalog.Title,
                RequiredPoints = x.RewardCatalog.RequiredPoints,
                RemainedPoints = x.User.RemainedPoints ?? 0,
                RequestDate = x.RequestDate,
                ReviewedDate = x.ReviewedDate,
                IsComplete = x.IsComplete,
                RewardDeliveryStatusID = x.RewardDeliveryStatusID,
                StatusTitle = x.RewardDeliveryStatus.Title
            });
        }
    }
}
