using Application.Services;
using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.User;
using Framework.Common;

namespace ApplicationService.Services
{
    public class UserService : IUserService
    {
        private const string DefaultProfileImageUrl = "/images/imageUsers/noimage.jpg";
        private readonly IUserRepository repo;

        public UserService(IUserRepository repo)
        {
            this.repo = repo;
        }

        public async Task<OperationResult> AddUser(UserAddEditModel user)
        {
            var op = new OperationResult("AddUser");
            try
            {
                if (user == null)
                    return op.ToFailed("اطلاعات کاربر نامعتبر است");

                if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                    return op.ToFailed("شماره موبایل اجباری است");

                user.IsDeleted = false;
                if (string.IsNullOrWhiteSpace(user.ProfileImageUrl))
                    user.ProfileImageUrl = DefaultProfileImageUrl;

                return await repo.Add(user);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ثبت کاربر: " + ex.Message);
            }
        }

        public async Task<OperationResult> UpdateUser(UserAddEditModel user)
        {
            var op = new OperationResult("UpdateUser");
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.UserID))
                    return op.ToFailed("شناسه کاربر نامعتبر است");

                if (!await repo.Exists(user.UserID))
                    return op.ToFailed("کاربر یافت نشد.");

                if (string.IsNullOrWhiteSpace(user.ProfileImageUrl))
                    user.ProfileImageUrl = DefaultProfileImageUrl;

                return await repo.Update(user);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ویرایش کاربر: " + ex.Message);
            }
        }

        public async Task<OperationResult> DeleteUser(string userID)
        {
            var op = new OperationResult("DeleteUser");
            try
            {
                if (string.IsNullOrWhiteSpace(userID) || !await repo.Exists(userID))
                    return op.ToFailed("کاربر یافت نشد");

                return await repo.Delete(userID);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف کاربر: " + ex.Message);
            }
        }

        public async Task<OperationResult> MergeUsers(string currentUserID, string mergeUserID)
        {
            var op = new OperationResult("MergeUsersInUserManagement");
            if (string.IsNullOrWhiteSpace(currentUserID) || string.IsNullOrWhiteSpace(mergeUserID))
                return op.ToFailed("لطفاً حساب مبدأ را انتخاب کنید.");
            if (currentUserID == mergeUserID)
                return op.ToFailed("امکان ادغام یک کاربر با خودش وجود ندارد.");
            return await repo.MergeAccounts(currentUserID, mergeUserID);
        }

        public Task<UserAddEditModel?> Get(string userID)
            => repo.Get(userID);

        public Task<UserDetailsModel?> GetDetails(string userID)
            => repo.GetDetails(userID);

        public Task<UserDetailsModel> GetUserDetail(string search)
            => repo.GetUserDetail(search);

        public Task<UserListComplexModel> Search(UserSearchModel sm)
        {
            sm ??= new UserSearchModel();
            return repo.Search(sm);
        }

        public Task<List<Province>> GetProvinces()
            => repo.GetProvinces();

        public Task<List<City>> GetCitiesByProvinceId(int provinceId)
            => repo.GetCitiesByProvinceId(provinceId);

        public Task<List<CustomerType>> GetCustomerTypes()
            => repo.GetCustomerTypes();
    }
}
