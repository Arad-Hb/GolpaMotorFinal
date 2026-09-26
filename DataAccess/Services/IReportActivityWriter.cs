using DomainModel.Models;

namespace DataAccess.Services
{
    public interface IReportActivityWriter
    {
        Task AddCardRegisteredAsync(
            string userId,
            WarrantyCard card,
            CardRegistration registration,
            PointTransaction transaction);

        Task AddRewardActivityAsync(
            RewardRequest request,
            string activityType,
            PointTransaction? transaction = null);
    }
}
