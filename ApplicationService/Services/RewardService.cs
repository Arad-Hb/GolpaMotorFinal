using Application.Services;
using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Reward;
using Framework.Common;

namespace ApplicationService.Services
{
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
        public Task<RewardCatalogListComplexModel> SearchCatalogs(RewardCatalogSearchModel sm)
            => catalogs.Search(sm ?? new RewardCatalogSearchModel());
        public Task<RewardRequestListComplexModel> SearchRequests(RewardRequestSearchModel sm)
            => requests.Search(sm ?? new RewardRequestSearchModel());

        public async Task<OperationResult> AddCatalog(RewardCatalogAddEditModel model)
        {
            var op = new OperationResult("AddCatalog");
            if (model == null || string.IsNullOrWhiteSpace(model.Title))
                return op.ToFailed("عنوان پاداش اجباری است");
            if (model.RequiredPoints <= 0)
                return op.ToFailed("حد نصاب باید بزرگ‌تر از صفر باشد");
            return await catalogs.Add(model);
        }

        public async Task<OperationResult> UpdateCatalog(RewardCatalogAddEditModel model)
        {
            var op = new OperationResult("UpdateCatalog");
            if (model == null || model.RewardCatalogID <= 0)
                return op.ToFailed("شناسه نامعتبر است");
            if (!await catalogs.Exists(model.RewardCatalogID))
                return op.ToFailed("پاداش پیدا نشد");
            return await catalogs.Update(model);
        }

        public async Task<OperationResult> DeleteCatalog(int rewardCatalogID)
        {
            var op = new OperationResult("DeleteCatalog");
            if (rewardCatalogID <= 0 || !await catalogs.Exists(rewardCatalogID))
                return op.ToFailed("پاداش پیدا نشد");
            return await catalogs.Delete(rewardCatalogID);
        }

        public Task<RewardCatalogAddEditModel?> GetCatalog(int rewardCatalogID) => catalogs.Get(rewardCatalogID);
        public Task<RewardCatalogDetailsModel?> GetCatalogDetails(int rewardCatalogID) => catalogs.GetDetails(rewardCatalogID);
        public Task<RewardRequestListItem?> GetRequestDetails(int rewardRequestID) => requests.GetDetails(rewardRequestID);

        public async Task<OperationResult> Approve(int rewardRequestID)
        {
            var op = new OperationResult("Approve");
            if (rewardRequestID <= 0)
                return op.ToFailed("شناسه نامعتبر است");
            return await requests.Approve(rewardRequestID);
        }

        public async Task<OperationResult> Reject(int rewardRequestID)
        {
            var op = new OperationResult("Reject");
            if (rewardRequestID <= 0)
                return op.ToFailed("شناسه نامعتبر است");
            return await requests.Reject(rewardRequestID);
        }

        public async Task<OperationResult> CreateRequest(string userId, int rewardCatalogId)
        {
            var op = new OperationResult("CreateRequest");
            if (string.IsNullOrWhiteSpace(userId) || rewardCatalogId <= 0)
                return op.ToFailed("اطلاعات درخواست نامعتبر است");
            return await requests.CreateRequest(userId, rewardCatalogId);
        }

        public Task RefreshEligibility(string userId) => requests.RefreshEligibility(userId);
        public Task<List<UserEligibleRewardItem>> GetEligibleCatalogsForUser(string userId)
            => requests.GetEligibleCatalogsForUser(userId);
        public Task<List<RewardRequestListItem>> GetUserRequests(string userId)
            => requests.GetUserRequests(userId);
        public Task<int> GetAvailablePoints(string userId) => requests.GetAvailablePoints(userId);
    }
}
