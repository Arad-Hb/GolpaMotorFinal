using DomainModel.Models;
using DomainModel.ViewModels.Reward;
using Framework.Common;

namespace DataAccess.Services
{
    public interface IRewardRequestRepository
    {
        Task<OperationResult> CreateRequest(string userId, int rewardCatalogId);
        Task<OperationResult> Approve(int rewardRequestId);
        Task<OperationResult> Reject(int rewardRequestId);
        Task<RewardRequestListItem?> GetDetails(int rewardRequestId);
        Task<RewardRequestListComplexModel> Search(RewardRequestSearchModel sm);
        Task<List<RewardRequestListItem>> GetUserRequests(string userId);
        Task<List<UserEligibleRewardItem>> GetEligibleCatalogsForUser(string userId);
        Task RefreshEligibility(string userId);
        Task<int> GetAvailablePoints(string userId);
        Task<List<RewardDeliveryStatus>> GetStatuses();
    }
}
