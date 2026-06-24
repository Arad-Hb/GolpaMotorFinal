using DataAccess.Services;
using DomainModel.ViewModels.User;
using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;


namespace GolpaMotorFinal.FrameworkUI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository repo;
        private readonly IFileManager fileManager;

        public UserService(IUserRepository repo , IFileManager fileManager)
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
                    var path = fileManager.ToPhysicalAddress(user.ProfileImageUrl, "ImageUsers");
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
                var saveResult = fileManager.SaveFile(imageFile, "ImageUsers", 2048, 2097152);

                if (!saveResult.Success)
                    return op.ToFailed(saveResult.Message);

                // تغییر مهم: فقط نام فایل ذخیره می‌شود
                user.ProfileImageUrl = saveResult.Message;

                user.IsDeleted = false;

                return await repo.Add(user);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ثبت محصول: " + ex.Message);
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
                    var saveResult = fileManager.SaveFile(imageFile, "ImageUsers", 2048, 2097152);

                    if (!saveResult.Success)
                        return op.ToFailed(saveResult.Message);

                    // تغییر: حذف فایل قبلی قبل از جایگزینی
                    if (!string.IsNullOrEmpty(current.ProfileImageUrl))
                    {
                        var oldPath = fileManager.ToPhysicalAddress(current.ProfileImageUrl, "ImageProducts");

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
                return op.ToFailed("خطا در ویرایش محصول: " + ex.Message);
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
                    return op.ToFailed("محصول یافت نشد");

                if (string.IsNullOrEmpty(user.ProfileImageUrl))
                    return op.ToFailed("تصویری وجود ندارد");

                var path = fileManager.ToPhysicalAddress(user.ProfileImageUrl, "ImageProducts");
                fileManager.RemoveFile(path);

                await repo.RemoveImage(userID);

                return op.ToSuccess("تصویر حذف شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف تصویر: " + ex.Message);
            }
        }
    }
}
