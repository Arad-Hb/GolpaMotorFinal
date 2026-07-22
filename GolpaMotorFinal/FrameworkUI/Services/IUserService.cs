
using DomainModel.ViewModels.User;
using Framework.Common;
using GolpaMotorFinal.Models.ViewModels.UserManagement;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public interface IUserService
    {
        Task<OperationResult> AddUser(UserAddEditModel user);
        Task<OperationResult> UpdateUser(UserAddEditModel user);
        Task<OperationResult> DeleteUser(string userID);
        Task<UserAddEditViewModel?> GetForEdit(string userID);
        Task<List<UserListItemViewModel>> GetUsers();
        Task<List<UserReportViewModel>> GetUserReport();
        Task<MergeAccountsViewModel> GetUserMergeAccounts(string userID);
        Task<MergeAccountsViewModel> GetMergeSearchResult(string sm);
        Task<OperationResult> MergeUsers(MergeAccountsViewModel model);

    }
}
