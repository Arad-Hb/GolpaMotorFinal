using DataAccess.Services;
using DomainModel.ViewModels.User;
using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace GolpaMotorFinal.FrameworkUI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository repo;
        private readonly IFileManager fileManager;

        public UserService(IUserRepository repo, IFileManager fileManager)
        {
            this.repo = repo;
            this.fileManager = fileManager;
        }

        public async Task<OperationResult> DeleteUser(string userID)
        {
            var op = new OperationResult("DeleteUser");

            try
            {
                var user = await repo.Get(userID);

                if (user == null)
                    return op.ToFailed("کاربر یافت نشد");

                // تغییر: اول DB حذف انجام می‌شود (امن‌تر)
                var result = await repo.Delete(userID);

                if (!result.Success)
                    return result;

                // تغییر: بعد از موفقیت DB، فایل حذف می‌شود
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var path = fileManager.Remove(user.ProfileImageUrl);
                }

                return result;
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف کاربر: " + ex.Message);
            }
        }

        public async Task<OperationResult> AddUser(UserAddEditModel user)
        {
            var op = new OperationResult("AddUser");

            try
            {
                user.IsDeleted = false;

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
                var currentUser = await repo.Get(user.UserID);

                if (currentUser == null)
                    return op.ToFailed("کاربر یافت نشد.");

                return await repo.Update(user);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ویرایش کاربر: " + ex.Message);
            }
        }

        public async Task<UserAddEditViewModel?> GetForEdit(string userID)
        {
            var user = await repo.Get(userID);

            if (user == null) return null;

            var form = new GolpaMotorFinal.Models.ViewModels.CrudFormViewModel
            {
                Title = "ویرایش کاربر",
                Controller = "UserManagement",
                Action = "Edit",
                Method = "POST",
                Enctype = "multipart/form-data",
                SubmitButtonText = "ثبت نهایی",
                CloseOnSuccess = true,
                RefreshGrid = true,
                GridId = "UserGrid",
                RefreshGridUrl = "Grid"
            };

            var vm = new UserAddEditViewModel
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProvinceID = user.ProvinceID,
                CityID = user.CityID,
                Address = user.Address,
                PostalCode = user.PostalCode,
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted,
                ProfileImageUrl = user.ProfileImageUrl,
                CrudFormViewModel = form
            };

            return vm;
        }


        public async Task<List<UserListItemViewModel>> GetUsers()
        {
            var users = await repo.GetAll();

            var result = new List<UserListItemViewModel>();

            foreach (var u in users)
            {
                var roleName = await repo.GetUserRoleById(u.UserID);

                result.Add(new UserListItemViewModel
                {
                    UserID = u.UserID,
                    ProfileImageUrl = u.ProfileImageUrl,
                    FullName = $"{u.FirstName ?? string.Empty} {u.LastName ?? string.Empty}".Trim(),
                    PhoneNumber = u.PhoneNumber ?? string.Empty,
                    RoleName = roleName,
                    TotalRegisteredCards = u.TotalRegisteredCards,
                    TotalEarnedPoints = u.TotalEarnedPoints,
                    Province = u.Province ?? string.Empty,
                    City = u.City ?? string.Empty
                });
            }

            return result;
        }

        public async Task<List<UserReportViewModel>> GetUserReport()
        {
            var users = await repo.GetAll();

            return users.Select(u => new UserReportViewModel
            {
                UserID = u.UserID,

                FullName = $"{u.FirstName ?? string.Empty} {u.LastName ?? string.Empty}".Trim(),

                PhoneNumber = u.PhoneNumber ?? string.Empty,
                RoleName = u.RoleName ?? string.Empty,

                TotalRegisteredCards = u.TotalRegisteredCards,
                TotalEarnedPoints = u.TotalEarnedPoints,
                TotalSettledPoints = u.TotalSettledPoints,
                RemainedPoints = u.RemainedPoints,

                Province = u.Province ?? string.Empty,
                City = u.City ?? string.Empty
            })
            .ToList();
        }
        public async Task<MergeAccountsViewModel> GetUserMergeAccounts(string userID)
        {
            var user = await repo.GetDetails(userID);

            var result = new MergeAccountsViewModel
            {
                UserID = user.UserID,
                FullName = user.FirstName + " " + user.LastName,
                PhoneNumber = user.PhoneNumber,
                RemainedPoints = user.RemainedPoints,
                ProfileImageUrl = user.ProfileImageUrl
            };
            return result;
        }
        public async Task<MergeAccountsViewModel> GetMergeSearchResult(string sm)
        {
            var result = new MergeAccountsViewModel();

            var searchResult = await repo.GetUserDetail(sm);

            if (searchResult!=null)
            {
                result = new MergeAccountsViewModel
                {
                    UserID = searchResult.UserID,
                    FullName = searchResult.FirstName + " " + searchResult.LastName,
                    PhoneNumber = searchResult.PhoneNumber,
                    RemainedPoints = searchResult.RemainedPoints,
                    ProfileImageUrl = searchResult.ProfileImageUrl
                };
            }
            return result;

        }

        public async Task<OperationResult> MergeUsers(MergeAccountsViewModel model)
        {
            var op = new OperationResult("MergeUsersInUserManagement");

            if (model == null)
                return op.ToFailed("اطلاعات نامعتبر است.");

            if (model.UserID == model.SelectedMergeUserID)
                return op.ToFailed("امکان ادغام یک کاربر با خودش وجود ندارد.");

            return await repo.MergeAccounts(model.UserID, model.SelectedMergeUserID);
        }
    }
}

