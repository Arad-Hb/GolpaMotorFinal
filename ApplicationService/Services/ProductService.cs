using Application.Services;
using DataAccess.Services;
using DomainModel.ViewModels.Product;
using Framework.Common;

namespace ApplicationService.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository repo;

        public ProductService(IProductRepository repo)
        {
            this.repo = repo;
        }

        public async Task<OperationResult> AddProduct(ProductAddEditModel prod)
        {
            var op = new OperationResult("AddProduct");
            try
            {
                if (prod == null)
                    return op.ToFailed("اطلاعات محصول نامعتبر است");
                if (string.IsNullOrWhiteSpace(prod.ProductName))
                    return op.ToFailed("نام محصول اجباری است");
                if (string.IsNullOrWhiteSpace(prod.ImageUrl))
                    return op.ToFailed("تصویر محصول الزامی است");

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
                if (prod == null || prod.ProductID <= 0)
                    return op.ToFailed("شناسه نامعتبر است");
                if (!await repo.Exists(prod.ProductID))
                    return op.ToFailed("محصول یافت نشد");
                if (string.IsNullOrWhiteSpace(prod.ImageUrl))
                {
                    var current = await repo.Get(prod.ProductID);
                    prod.ImageUrl = current?.ImageUrl;
                }

                return await repo.Update(prod);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ویرایش محصول: " + ex.Message);
            }
        }

        public async Task<OperationResult> DeleteProduct(long productID)
        {
            var op = new OperationResult("DeleteProduct");
            try
            {
                if (!await repo.Exists(productID))
                    return op.ToFailed("محصول یافت نشد");
                return await repo.Delete(productID);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف محصول: " + ex.Message);
            }
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

                await repo.RemoveImage(productID);
                return op.ToSuccess("تصویر حذف شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف تصویر: " + ex.Message);
            }
        }

        public Task<ProductAddEditModel?> Get(long productID) => repo.Get(productID);
        public Task<ProductDetailsModel?> GetDetails(long productID) => repo.GetDetails(productID);
        public Task<ProductStatistics> GetStatistics() => repo.GetStatistics();
        public Task<ProductListComplexModel> Search(ProductSearchModel sm) => repo.Search(sm ?? new ProductSearchModel());
        public Task<List<ProductListItem>> GetAll() => repo.GetAll();
        public Task<List<NamedCountItem>> GetTopRegistrars(int take = 5) => repo.GetTopRegistrars(take);
        public Task<(List<NamedCountItem> Items, int Total)> GetTopRegistrarsPage(int pageIndex, int pageSize = 10)
            => repo.GetTopRegistrarsPage(pageIndex, pageSize);
    }
}
