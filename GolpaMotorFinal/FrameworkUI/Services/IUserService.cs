
using DomainModel.ViewModels.User;
using Framework.Common;
using GolpaMotorFinal.Models.ViewModels.UserManagement;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public interface IUserService
    {
        Task<OperationResult> AddUser(UserAddEditModel user, IFormFile imageFile);
        Task<OperationResult> UpdateUser(UserAddEditModel user, IFormFile? imageFile);
        Task<OperationResult> DeleteUser(string userID);
        Task<UserAddEditModel?> GetForEdit(string userID);
        Task<List<UserListItemViewModel>> GetUsers();
        Task<List<UserReportViewModel>> GetUserReport();
        Task<MergeAccountsViewModel> GetUserMergeAccounts(string userID);
        Task<MergeAccountsViewModel> GetMergeSearchResult(string sm);

        Task<OperationResult> MergeUsers(MergeAccountsViewModel model);
        Task<OperationResult> RemovePicture(string userID);
    }
}
