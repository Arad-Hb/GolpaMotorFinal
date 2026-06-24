using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;
using System.Net.NetworkInformation;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public class FileManager : IFileManager
    {
        private readonly IHostEnvironment env;
        public FileManager(IHostEnvironment env)
        {
            this.env = env;
        }
        public OperationResult ValidateFileSize(IFormFile file, long MinCapacity, long MaxCapacity)
        {
            OperationResult op = new OperationResult("ValidateFileSize");
            if (file.Length < MinCapacity || file.Length > MaxCapacity)
            {
                return op.ToFailed("Invalid File Size");
            }
            else
            {
                return op.ToSuccess("File Size Is Valid");
            }
        }
        public bool ValidateFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false; // نام فایل خالی یا فقط فاصله است
            }

            fileName = fileName.Trim().ToLower();

            // گرفتن extension واقعی فایل
            var ext = Path.GetExtension(fileName);

            // پسوندهای مجاز فقط تصاویر
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

            if (!allowedExtensions.Contains(ext))
                return false;

            //  جلوگیری از کاراکترهای غیرمجاز ویندوز
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return false;

            //  محدودیت طول نام فایل
            if (fileName.Length > 255)
                return false;

            return true;
        }
        public string ToUniqueFileName(string fileName)
        {
            return Guid.NewGuid().ToString().Replace("-", "_")+"_" + fileName;
        }
        public string ToPhysicalAddress(string fileName,string folderName)
        {
            return env.ContentRootPath + @"\wwwroot\" + folderName + @"\" + fileName;
        }
        
        public string ToRelativeAddress(string UniqueFileName, string Folder)
        {
            return @"~/" + Folder + @"/" + UniqueFileName;
        }

        //relative path
        public bool RemoveFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (!File.Exists(path))
                return false;

            File.Delete(path);
            return true;
        }

        //url relative
        public OperationResult SaveFile(IFormFile file, string folderName, long minSize, long maxSize)
        {
            var op = new OperationResult("SaveFile");

            if (file == null || file.Length == 0)
                return op.ToFailed("فایل معتبر نیست");

            var fileName = Path.GetFileName(file.FileName);

            // تغییر: validation کامل اسم فایل داخل FileManager انجام می‌شود
            if (!ValidateFileName(fileName))
                return op.ToFailed("نام یا فرمت فایل نامعتبر است");

            // تغییر: validation سایز هم داخل FileManager انجام می‌شود
            var sizeCheck = ValidateFileSize(file, minSize, maxSize);
            if (!sizeCheck.Success)
                return sizeCheck;

            var uniqueFileName = ToUniqueFileName(fileName);
            var path = ToPhysicalAddress(uniqueFileName, folderName);

            try
            {
                using var fs = new FileStream(path, FileMode.Create);
                file.CopyTo(fs);

                // فقط نام فایل برمی‌گردد (نه مسیر کامل)
                return op.ToSuccess(uniqueFileName);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ذخیره فایل: " + ex.Message);
            }
        }
    }
}
