using DataAccess.Repositories;
using DataAccess.Services;
using DomainModel.ViewModels.User;
using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using Microsoft.EntityFrameworkCore;
using static GolpaMotorFinal.FrameworkUI.Services.UserService;


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
                    var path = fileManager.ToPhysicalAddress(user.ProfileImageUrl, "~/images/imageUsers");
                    fileManager.RemoveFile(path);
                }

                return result;
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف کاربر: " + ex.Message);
            }
        }

        public async Task<OperationResult> AddUser(UserAddEditModel user, IFormFile imageFile)
        {
            var op = new OperationResult("AddUser");

            try
            {
                if (imageFile == null)
                    return op.ToFailed("تصویر کاربر الزامی است");

                // تغییر: همه validation ها داخل FileManager انجام می‌شود
                var saveResult = fileManager.SaveFile(imageFile, "~/images/imageUsers", 2048, 2097152);

                if (!saveResult.Success)
                    return op.ToFailed(saveResult.Message);

                // تغییر مهم: فقط نام فایل ذخیره می‌شود
                user.ProfileImageUrl = saveResult.Message;

                user.IsDeleted = false;

                return await repo.Add(user);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ثبت کاربر: " + ex.Message);
            }
        }

        public async Task<OperationResult> UpdateUser(UserAddEditModel user, IFormFile? imageFile)
        {
            var op = new OperationResult("UpdateUser");

            try
            {
                var current = await repo.Get(user.UserID);

                if (current == null)
                    return op.ToFailed("کاربر یافت نشد");

                if (imageFile != null)
                {
                    // تغییر: FileManager مسئول save + validation
                    var saveResult = fileManager.SaveFile(imageFile, "~/images/imageUsers", 2048, 2097152);

                    if (!saveResult.Success)
                        return op.ToFailed(saveResult.Message);

                    // تغییر: حذف فایل قبلی قبل از جایگزینی
                    if (!string.IsNullOrEmpty(current.ProfileImageUrl))
                    {
                        var oldPath = fileManager.ToPhysicalAddress(current.ProfileImageUrl, "~/images/imageUsers");

                        // تغییر: حذف امن فایل قبلی
                        fileManager.RemoveFile(oldPath);
                    }

                    // تغییر مهم: فقط نام فایل جدید
                    user.ProfileImageUrl = saveResult.Message;
                }
                else
                {
                    // تغییر: اگر عکس جدید نیامد، قبلی حفظ می‌شود
                    user.ProfileImageUrl = current.ProfileImageUrl;
                }

                return await repo.Update(user);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ویرایش کاربر: " + ex.Message);
            }
        }

        public async Task<UserAddEditModel?> GetForEdit(string userID)
        {
            return await repo.Get(userID);
        }

        public async Task<OperationResult> RemovePicture(string userID)
        {
            var op = new OperationResult("RemovePicture");

            try
            {
                var user = await repo.Get(userID);

                if (user == null)
                    return op.ToFailed("کاربر یافت نشد");

                if (string.IsNullOrEmpty(user.ProfileImageUrl))
                    return op.ToFailed("تصویری وجود ندارد");

                var path = fileManager.ToPhysicalAddress(user.ProfileImageUrl, "~/images/imageUsers");
                fileManager.RemoveFile(path);

                await repo.RemoveImage(userID);

                return op.ToSuccess("تصویر حذف شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف تصویر: " + ex.Message);
            }
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

