
using DomainModel.ViewModels.User;
using Framework.Common;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public interface IUserService
    {
        Task<OperationResult> AddUser(UserAddEditModel user, IFormFile imageFile);
        Task<OperationResult> UpdateUser(UserAddEditModel user, IFormFile? imageFile);
        Task<OperationResult> DeleteUser(string userID);
        Task<UserAddEditModel?> GetForEdit(string userID);
        Task<OperationResult> RemovePicture(string userID);
    }
}
