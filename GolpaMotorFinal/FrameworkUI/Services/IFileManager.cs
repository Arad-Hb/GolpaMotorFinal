using Framework.Common;
using GolpaMotorFinal.Models;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public interface IFileManager
    {
        Task<FileUploadResult> UploadAsync(
            IFormFile file,
            int maxSizeMB,
            string[] allowedExtensions,
            string uploadFolder,
            string thumbnailFolder);

        bool Remove(string relativePath);






        //string ToUniqueFileName(string fileName);
        //string ToPhysicalAddress(string fileName,string folderName);
        //string ToRelativeAddress(string UniqueFileName, string Folder);
        //bool ValidateFileName(string fileName);
        //bool RemoveFile(string path);        
        //OperationResult ValidateFileSize(IFormFile file,long MinCapacity, long MaxCapacity);

        ////OperationResult SaveFile(IFormFile file, string folderName); 
        //OperationResult SaveFile(IFormFile file, string folderName, long minSize, long maxSize);
    }
}
