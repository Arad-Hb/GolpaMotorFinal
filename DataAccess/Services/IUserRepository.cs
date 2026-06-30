using DomainModel.Models;
using DomainModel.ViewModels;
using DomainModel.ViewModels.Product;
using DomainModel.ViewModels.User;
using Framework.Common;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Services
{
    public interface IUserRepository
    {
        Task<OperationResult> Add(UserAddEditModel user);
        Task<OperationResult> Update(UserAddEditModel user);
        Task<OperationResult> Delete(string UserID);
        Task<OperationResult> SoftDelete(string userID);
        Task<OperationResult> MergeAccounts(string currentUserID, string mergeUserID);
        Task<UserAddEditModel?> Get(string userID);
        Task<UserDetailsModel> GetUserDetail(string sm);
        Task<List<UserDetailsModel>> GetAll();
        Task<string> GetUserRoleById(string userID);
        Task<UserDetailsModel?> GetDetails(string userID);
        Task<bool> Exists(string userID);
        Task<UserListComplexModel> Search(UserSearchModel sm);
        Task RemoveImage(string userID);
        Task<List<Province>> GetProvinces();
        Task<List<City>> GetCitiesByProvinceId(int provinceId);
    }
}
