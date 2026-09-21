using DomainModel.Models;
using DomainModel.ViewModels.Reward;
using Framework.Common;

namespace Application.Services
{
    public interface IRewardService
    {
        Task<List<RewardCatalogListItem>> GetCatalogs();
        Task<List<RewardDeliveryStatus>> GetStatuses();
        Task<RewardCatalogListComplexModel> SearchCatalogs(RewardCatalogSearchModel sm);
        Task<RewardRequestListComplexModel> SearchRequests(RewardRequestSearchModel sm);
        Task<OperationResult> AddCatalog(RewardCatalogAddEditModel model);
        Task<OperationResult> UpdateCatalog(RewardCatalogAddEditModel model);
        Task<OperationResult> DeleteCatalog(int rewardCatalogID);
        Task<RewardCatalogAddEditModel?> GetCatalog(int rewardCatalogID);
        Task<RewardCatalogDetailsModel?> GetCatalogDetails(int rewardCatalogID);
        Task<RewardRequestListItem?> GetRequestDetails(int rewardRequestID);
        Task<OperationResult> Approve(int rewardRequestID);
        Task<OperationResult> Reject(int rewardRequestID);
        Task<OperationResult> CreateRequest(string userId, int rewardCatalogId);
        Task RefreshEligibility(string userId);
        Task<List<UserEligibleRewardItem>> GetEligibleCatalogsForUser(string userId);
        Task<List<RewardRequestListItem>> GetUserRequests(string userId);
        Task<int> GetAvailablePoints(string userId);
    }
}
