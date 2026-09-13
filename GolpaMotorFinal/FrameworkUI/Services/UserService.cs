using DataAccess.Services;
using DomainModel.ViewModels.User;
using Framework.Common;
using GolpaMotorFinal.Models;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.CRUD;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using Microsoft.AspNetCore.Mvc.Rendering;

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

                var result = await repo.Delete(userID);
                if (!result.Success)
                    return result;

                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                    fileManager.Remove(user.ProfileImageUrl);

                return result;
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف کاربر: " + ex.Message);
            }
        }

        public Task<OperationResult> AddUser(UserAddEditModel user)
            => AddUser(user, null);

        public async Task<OperationResult> AddUser(UserAddEditModel user, IFormFile? image)
        {
            var op = new OperationResult("AddUser");
            try
            {
                user.IsDeleted = false;
                var uploaded = await TryUpload(image);
                if (uploaded is { Success: false })
                    return op.ToFailed(uploaded.Message);
                if (uploaded is { Success: true })
                    user.ProfileImageUrl = uploaded.FileUrl;
                else if (string.IsNullOrWhiteSpace(user.ProfileImageUrl))
                    user.ProfileImageUrl = "/images/imageUsers/noimage.jpg";

                var result = await repo.Add(user);
                if (!result.Success && uploaded is { Success: true })
                    RemoveUserImage(uploaded.FileUrl);
                return result;
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ثبت کاربر: " + ex.Message);
            }
        }

        public Task<OperationResult> UpdateUser(UserAddEditModel user)
            => UpdateUser(user, null);

        public async Task<OperationResult> UpdateUser(UserAddEditModel user, IFormFile? image)
        {
            var op = new OperationResult("UpdateUser");
            try
            {
                var currentUser = await repo.Get(user.UserID);
                if (currentUser == null)
                    return op.ToFailed("کاربر یافت نشد.");

                user.ProfileImageUrl = string.IsNullOrWhiteSpace(currentUser.ProfileImageUrl)
                    ? "/images/imageUsers/noimage.jpg"
                    : currentUser.ProfileImageUrl;

                var uploaded = await TryUpload(image);
                if (uploaded is { Success: false })
                    return op.ToFailed(uploaded.Message);
                if (uploaded is { Success: true })
                    user.ProfileImageUrl = uploaded.FileUrl;

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

            var form = new CrudFormViewModel
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
                RefreshGridUrl = "List"
            };

            var provinces = await repo.GetProvinces();
            var customerTypes = await repo.GetCustomerTypes();
            var cities = user.ProvinceID.HasValue
                ? await repo.GetCitiesByProvinceId(user.ProvinceID.Value)
                : new List<DomainModel.Models.City>();

            return new UserAddEditViewModel
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CustomerTypeID = user.CustomerTypeID,
                ProvinceID = user.ProvinceID,
                CityID = user.CityID,
                Address = user.Address,
                PostalCode = user.PostalCode,
                CreditCartNumber = user.CreditCartNumber,
                IBAN = user.IBAN,
                AccountNumber = user.AccountNumber,
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted,
                ProfileImageUrl = user.ProfileImageUrl,
                CustomerTypes = new SelectList(customerTypes, "CustomerTypeID", "Title", user.CustomerTypeID),
                Provinces = new SelectList(provinces, "ProvinceID", "Name", user.ProvinceID),
                Cities = new SelectList(cities, "CityID", "Name", user.CityID),
                CrudFormViewModel = form
            };
        }

        public async Task<(List<UserListItemViewModel> Items, int PageIndex, int PageCount, int RecordCount)> GetListPage(UserSearchModel sm)
        {
            sm ??= new UserSearchModel();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var result = await repo.Search(sm);
            var page = result.sm ?? sm;
            var items = (result.userList ?? new List<UserListItem>()).Select(MapListItem).ToList();
            return (items, page.PageIndex, page.PageCount, page.RecordCount);
        }

        public async Task<(List<UserReportViewModel> Users, int PageIndex, int PageCount, int RecordCount)> GetUserReportPage(UserSearchModel? sm = null)
        {
            sm ??= new UserSearchModel();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var result = await repo.Search(sm);
            var page = result.sm ?? sm;
            var users = (result.userList ?? new List<UserListItem>()).Select(u => new UserReportViewModel
            {
                UserID = u.UserID,
                FullName = $"{u.FirstName ?? string.Empty} {u.LastName ?? string.Empty}".Trim(),
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                RoleName = u.RoleName ?? string.Empty,
                TotalRegisteredCards = u.TotalRegisteredCards,
                TotalEarnedPoints = u.TotalEarnedPoints,
                TotalSettledPoints = u.TotalSettledPoints,
                RemainedPoints = u.RemainedPoints,
                ProfileImageUrl = u.ExistingProfileImageUrl ?? string.Empty,
                Province = u.Province ?? string.Empty,
                City = u.City ?? string.Empty
            }).ToList();

            return (users, page.PageIndex, page.PageCount, page.RecordCount);
        }

        public async Task<MergeAccountsViewModel> GetUserMergeAccounts(string userID)
        {
            var user = await repo.GetDetails(userID);
            if (user == null)
                return new MergeAccountsViewModel();

            return new MergeAccountsViewModel
            {
                UserID = user.UserID,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                PhoneNumber = user.PhoneNumber,
                TotalEarnedPoints = user.TotalEarnedPoints,
                TotalSettledPoints = user.TotalSettledPoints,
                RemainedPoints = user.RemainedPoints,
                TotalRegisteredCards = user.TotalRegisteredCards,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }

        public async Task<MergeAccountsViewModel> GetMergeSearchResult(string sm)
        {
            var searchResult = await repo.GetUserDetail(sm);
            if (searchResult == null)
                return new MergeAccountsViewModel();

            return new MergeAccountsViewModel
            {
                UserID = searchResult.UserID,
                FullName = searchResult.FirstName + " " + searchResult.LastName,
                PhoneNumber = searchResult.PhoneNumber,
                TotalEarnedPoints = searchResult.TotalEarnedPoints,
                TotalSettledPoints = searchResult.TotalSettledPoints,
                RemainedPoints = searchResult.RemainedPoints,
                TotalRegisteredCards = searchResult.TotalRegisteredCards,
                ProfileImageUrl = searchResult.ProfileImageUrl
            };
        }

        public async Task<OperationResult> MergeUsers(MergeAccountsViewModel model)
        {
            var op = new OperationResult("MergeUsersInUserManagement");
            if (model == null)
                return op.ToFailed("اطلاعات نامعتبر است.");
            if (string.IsNullOrWhiteSpace(model.SelectedMergeUserID))
                return op.ToFailed("لطفاً حساب مبدأ را انتخاب کنید.");
            if (model.UserID == model.SelectedMergeUserID)
                return op.ToFailed("امکان ادغام یک کاربر با خودش وجود ندارد.");
            return await repo.MergeAccounts(model.UserID, model.SelectedMergeUserID);
        }

        private async Task<FileUploadResult?> TryUpload(IFormFile? image)
        {
            if (image == null)
                return null;
            return await fileManager.UploadAsync(
                image, 5, new[] { "jpg", "jpeg", "png" },
                "images/imageUsers/uploads", "images/imageUsers/thumbnails");
        }

        private void RemoveUserImage(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;
            fileManager.Remove(url);
            fileManager.Remove(url.Replace("/images/imageUsers/uploads/", "/images/imageUsers/thumbnails/"));
        }

        private static UserListItemViewModel MapListItem(UserListItem u)
        {
            return new UserListItemViewModel
            {
                UserID = u.UserID,
                FullName = $"{u.FirstName ?? string.Empty} {u.LastName ?? string.Empty}".Trim(),
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                ProfileImageUrl = u.ExistingProfileImageUrl ?? string.Empty,
                RoleName = u.RoleName ?? string.Empty,
                TotalRegisteredCards = u.TotalRegisteredCards,
                TotalEarnedPoints = u.TotalEarnedPoints,
                IsEligibleForReward = u.IsEligibleForReward,
                HasReceivedReward = u.HasReceivedReward,
                Province = u.Province ?? string.Empty,
                City = u.City ?? string.Empty
            };
        }
    }
}
