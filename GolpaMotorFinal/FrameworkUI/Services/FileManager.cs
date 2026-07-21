using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Models;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Net.NetworkInformation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public class FileManager : IFileManager
    {
        private readonly IWebHostEnvironment _environment;

        public FileManager(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<FileUploadResult> UploadAsync(
            IFormFile file,
            int maxSizeMB,
            string[] allowedExtensions,
            string uploadFolder,
            string thumbnailFolder)
        {
            var result = new FileUploadResult();

            if (file == null || file.Length == 0)
            {
                result.Message = "No file selected.";
                return result;
            }

            var extension = Path.GetExtension(file.FileName)
                .ToLower()
                .Replace(".", "");

            if (!allowedExtensions.Contains(extension))
            {
                result.Message = "Invalid file format.";
                return result;
            }

            long maxBytes = maxSizeMB * 1024L * 1024L;

            if (file.Length > maxBytes)
            {
                result.Message = "File size exceeds limit.";
                return result;
            }

            string uploads = Path.Combine(_environment.WebRootPath, uploadFolder);

            string thumbs = Path.Combine(_environment.WebRootPath, thumbnailFolder);

            Directory.CreateDirectory(uploads);
            Directory.CreateDirectory(thumbs);

            string fileName = Guid.NewGuid().ToString("N") + "." + extension;

            string filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (extension == "jpg" ||
                extension == "jpeg" ||
                extension == "png" ||
                extension == "gif")
            {
                using var image = await Image.LoadAsync(filePath);

                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(120, 120),
                    Mode = ResizeMode.Crop
                }));

                await image.SaveAsJpegAsync(
                    Path.Combine(thumbs, fileName),
                    new JpegEncoder());
            }

            result.Success = true;
            result.Message = "Uploaded successfully.";
            result.FileName = fileName;
            result.FileUrl = "/" + uploadFolder + "/" + fileName;
            result.ThumbnailUrl = "/" + thumbnailFolder + "/" + fileName;

            return result;
        }

        public bool Remove(string relativePath)
        {
            var path = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return true;
        }

    }
}
