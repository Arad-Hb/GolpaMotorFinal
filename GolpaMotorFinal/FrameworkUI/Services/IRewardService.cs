using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Reward;
using Framework.Common;

namespace GolpaMotorFinal.FrameworkUI.Services
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
    }

    public class RewardService : IRewardService
    {
        private readonly IRewardCatalogRepository catalogs;
        private readonly IRewardRequestRepository requests;

        public RewardService(IRewardCatalogRepository catalogs, IRewardRequestRepository requests)
        {
            this.catalogs = catalogs;
            this.requests = requests;
        }

        public Task<List<RewardCatalogListItem>> GetCatalogs() => catalogs.GetAll();
        public Task<List<RewardDeliveryStatus>> GetStatuses() => requests.GetStatuses();
        public Task<RewardCatalogListComplexModel> SearchCatalogs(RewardCatalogSearchModel sm) => catalogs.Search(sm);
        public Task<RewardRequestListComplexModel> SearchRequests(RewardRequestSearchModel sm) => requests.Search(sm);
        public Task<OperationResult> AddCatalog(RewardCatalogAddEditModel model) => catalogs.Add(model);
        public Task<OperationResult> UpdateCatalog(RewardCatalogAddEditModel model) => catalogs.Update(model);
        public Task<OperationResult> DeleteCatalog(int rewardCatalogID) => catalogs.Delete(rewardCatalogID);
        public Task<RewardCatalogAddEditModel?> GetCatalog(int rewardCatalogID) => catalogs.Get(rewardCatalogID);
        public Task<RewardCatalogDetailsModel?> GetCatalogDetails(int rewardCatalogID) => catalogs.GetDetails(rewardCatalogID);
        public Task<RewardRequestListItem?> GetRequestDetails(int rewardRequestID) => requests.GetDetails(rewardRequestID);
        public Task<OperationResult> Approve(int rewardRequestID) => requests.Approve(rewardRequestID);
        public Task<OperationResult> Reject(int rewardRequestID) => requests.Reject(rewardRequestID);
    }
}
