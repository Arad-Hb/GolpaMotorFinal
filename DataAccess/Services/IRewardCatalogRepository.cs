using DomainModel.ViewModels.Reward;
using Framework.Common;

namespace DataAccess.Services
{
    public interface IRewardCatalogRepository
    {
        Task<OperationResult> Add(RewardCatalogAddEditModel catalog);
        Task<OperationResult> Update(RewardCatalogAddEditModel catalog);
        Task<OperationResult> Delete(int rewardCatalogID);
        Task<RewardCatalogAddEditModel?> Get(int rewardCatalogID);
        Task<List<RewardCatalogListItem>> GetAll();
        Task<List<RewardCatalogListItem>> GetActiveCatalogs();
        Task<RewardCatalogDetailsModel?> GetDetails(int rewardCatalogID);
        Task<bool> Exists(int rewardCatalogID);
        Task<RewardCatalogListComplexModel> Search(RewardCatalogSearchModel sm);
    }
}
