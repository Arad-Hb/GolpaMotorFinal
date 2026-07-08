using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Product;
using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;

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

        public async Task<OperationResult> DeleteProduct(long productID)
        {
            var op = new OperationResult("DeleteProduct");

            try
            {
                var product = await repo.Get(productID);

                if (product == null)
                    return op.ToFailed("محصول یافت نشد");

                // تغییر: اول DB حذف انجام می‌شود (امن‌تر)
                var result = await repo.Delete(productID);

                if (!result.Success)
                    return result;

                // تغییر: بعد از موفقیت DB، فایل حذف می‌شود
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {

                    fileManager.Remove(product.ImageUrl);
                }

                return result;
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف محصول: " + ex.Message);
            }
        }

        public async Task<OperationResult> AddProduct(ProductAddEditModel prod, IFormFile imageFile)
        {
            var op = new OperationResult("AddProduct");

            try
            {
                if (imageFile == null)
                    return op.ToFailed("تصویر محصول الزامی است");

                //prod.ImageUrl = saveResult.Message;

                prod.IsDeleted = false;

                return await repo.Add(prod);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ثبت محصول: " + ex.Message);
            }
        }

        public async Task<OperationResult> UpdateProduct(ProductAddEditModel prod)
        {
            var op = new OperationResult("UpdateProduct");

            try
            {
                var current = await repo.Get(prod.ProductID);

                if (current == null)
                    return op.ToFailed("محصول یافت نشد");

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

                //var path = fileManager.ToPhysicalAddress(product.ImageUrl, "ImageProducts");
                //fileManager.RemoveFile(path);

                //await repo.RemoveImage(productID);

                return op.ToSuccess("تصویر حذف شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف تصویر: " + ex.Message);
            }
        }

        public Task<OperationResult> UpdateProduct(ProductAddEditModel prod, IFormFile? imageFile)
        {
            throw new NotImplementedException();
        }
    }
}