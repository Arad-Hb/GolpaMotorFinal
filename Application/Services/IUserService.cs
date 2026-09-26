using DomainModel.ViewModels.User;
using Framework.Common;

namespace Application.Services
{
    public interface IUserService
    {
        Task<OperationResult> AddUser(UserAddEditModel user);
        Task<OperationResult> UpdateUser(UserAddEditModel user);
        Task<OperationResult> DeleteUser(string userID);
        Task<OperationResult> MergeUsers(string currentUserID, string mergeUserID);
        Task<UserAddEditModel?> Get(string userID);
        Task<UserDetailsModel?> GetDetails(string userID);
        Task<UserDetailsModel> GetUserDetail(string search);
        Task<UserListComplexModel> Search(UserSearchModel sm);
    }
}
