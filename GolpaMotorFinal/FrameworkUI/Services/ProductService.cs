using DataAccess.Services;
using DomainModel.ViewModels.Product;
using Framework.Common;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository repo;
        private readonly IFileManager fileManager;

        public ProductService(IProductRepository repo, IFileManager fileManager)
        {
            this.repo = repo;
            this.fileManager = fileManager;
        }

        private static readonly string[] ImageExtensions = { "jpg", "jpeg", "png" };
        private const string ProductUploadFolder = "images/imageProducts/uploads";
        private const string ProductThumbFolder = "images/imageProducts/thumbnails";

        public async Task<OperationResult> DeleteProduct(long productID)
        {
            var op = new OperationResult("DeleteProduct");

            try
            {
                var product = await repo.Get(productID);

                if (product == null)
                    return op.ToFailed("محصول یافت نشد");

                var result = await repo.Delete(productID);

                if (!result.Success)
                    return result;

                if (!string.IsNullOrEmpty(product.ImageUrl))
                    fileManager.Remove(product.ImageUrl);

                return result;
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف محصول: " + ex.Message);
            }
        }

        public async Task<OperationResult> AddProduct(ProductAddEditModel prod, IFormFile? imageFile)
        {
            var op = new OperationResult("AddProduct");

            try
            {
                if (imageFile == null)
                    return op.ToFailed("تصویر محصول الزامی است");

                var upload = await fileManager.UploadAsync(
                    imageFile,
                    5,
                    ImageExtensions,
                    ProductUploadFolder,
                    ProductThumbFolder);

                if (!upload.Success)
                    return op.ToFailed(upload.Message);

                prod.ImageUrl = upload.FileUrl;
                prod.IsDeleted = false;

                return await repo.Add(prod);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ثبت محصول: " + ex.Message);
            }
        }

        public async Task<OperationResult> UpdateProduct(ProductAddEditModel prod, IFormFile? imageFile)
        {
            var op = new OperationResult("UpdateProduct");

            try
            {
                var current = await repo.Get(prod.ProductID);

                if (current == null)
                    return op.ToFailed("محصول یافت نشد");

                if (imageFile != null)
                {
                    var upload = await fileManager.UploadAsync(
                        imageFile,
                        5,
                        ImageExtensions,
                        ProductUploadFolder,
                        ProductThumbFolder);

                    if (!upload.Success)
                        return op.ToFailed(upload.Message);

                    if (!string.IsNullOrWhiteSpace(current.ImageUrl))
                        fileManager.Remove(current.ImageUrl);

                    prod.ImageUrl = upload.FileUrl;
                }
                else if (string.IsNullOrWhiteSpace(prod.ImageUrl))
                {
                    prod.ImageUrl = current.ImageUrl;
                }

                return await repo.Update(prod);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ویرایش محصول: " + ex.Message);
            }
        }

        public async Task<ProductAddEditModel?> GetForEdit(int productID)
        {
            return await repo.Get(productID);
        }

        public async Task<OperationResult> RemovePicture(long productID)
        {
            var op = new OperationResult("RemovePicture");

            try
            {
                var product = await repo.Get(productID);

                if (product == null)
                    return op.ToFailed("محصول یافت نشد");

                if (string.IsNullOrEmpty(product.ImageUrl))
                    return op.ToFailed("تصویری وجود ندارد");

                fileManager.Remove(product.ImageUrl);
                await repo.RemoveImage(productID);

                return op.ToSuccess("تصویر حذف شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف تصویر: " + ex.Message);
            }
        }
    }
}
