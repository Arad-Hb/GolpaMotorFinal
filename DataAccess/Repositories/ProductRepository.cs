using DataAccess.Mappers;
using DataAccess.Queries;
using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Product;
using Framework.Common;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class ProductRepository : IProductRepository
    {

        private readonly GolpaMotorDbContext db;

        public ProductRepository(GolpaMotorDbContext db)
        {
            this.db = db;
        }
        public async Task<OperationResult> Add(ProductAddEditModel product)
        {
            var op = new OperationResult("Add Product");

            try
            {
                var p = ProductMapper.ToEntity(product);

                db.Products.Add(p);
                await db.SaveChangesAsync();

                return op.ToSuccess("محصول با موفقیت اضافه شد", p.ProductID);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ثبت محصول: " + ex.Message);
            }
        }

        public async Task<OperationResult> Update(ProductAddEditModel product)
        {
            var op = new OperationResult("Update Product");

            if (product.ProductID <= 0)
                return op.ToFailed("شناسه نامعتبر است");

            try
            {
                var prod = await db.Products.FirstOrDefaultAsync(x => x.ProductID == product.ProductID && !x.IsDeleted);

                if (prod == null)
                    return op.ToFailed("محصول پیدا نشد");

                ProductMapper.Apply(prod, product);

                await db.SaveChangesAsync();

                return op.ToSuccess("محصول با موفقیت ویرایش شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ویرایش محصول: " + ex.Message);
            }
        }

        public async Task<OperationResult> Delete(long productID)
        {
            var op = new OperationResult("Delete Product");

            try
            {
                var product = await db.Products
                    .FirstOrDefaultAsync(x => x.ProductID == productID);

                if (product == null || product.IsDeleted)
                    return op.ToFailed("محصول پیدا نشد");

                product.IsDeleted = true;

                await db.SaveChangesAsync();

                return op.ToSuccess("محصول با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف محصول: " + ex.Message);
            }
        }
        
        public async Task<ProductAddEditModel?> Get(long productID)
        {
            var product = await ProductQueries.Active(db)
                .FirstOrDefaultAsync(x => x.ProductID == productID);

            if (product == null)
                return null;

            return ProductMapper.ToAddEditModel(product);
        }

        public async Task<List<ProductListItem>> GetAll()
        {
            return await ProductQueries.Active(db)
                .Select(ProductMapper.ToListItem)
                .ToListAsync();
        }

        public async Task<ProductDetailsModel?> GetDetails(long productID)
        {
            return await ProductQueries.Active(db)
                .Where(x => x.ProductID == productID)
                .Select(ProductMapper.ToDetails)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> Exists(long id)
        {
            return await ProductQueries.Active(db).AnyAsync(x => x.ProductID == id);
        }

        public async Task<ProductListComplexModel> Search(ProductSearchModel searchModel)
        {
            var result = new ProductListComplexModel();

            var query = ProductQueries.ApplySearch(ProductQueries.Active(db), searchModel);

            // 6. Count کل رکوردها (قبل از paging)
            var totalCount = await query.CountAsync();

            // 7. اصلاح PageIndex (جلوگیری از خطا)
            var pageIndex = searchModel.PageIndex < 0 ? 0 : searchModel.PageIndex;
            var pageSize = searchModel.PageSize <= 0 ? 10 : searchModel.PageSize;

            // 8. گرفتن دیتا با Paging
            var list = await query
                .OrderByDescending(p => p.ProductID)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(ProductMapper.ToListItem)
                .ToListAsync();

            // 9. خروجی نهایی
            result.sm = searchModel;
            result.sm.RecordCount = totalCount;
            result.productList = list;

            return result;
        }

        public async Task RemoveImage(long productID)
        {
            var product = await db.Products
                .FirstOrDefaultAsync(x => x.ProductID == productID);

            if (product != null)
            {
                product.ImageUrl = null;
                await db.SaveChangesAsync();
            }
        }

        public async Task<ProductStatistics> GetStatistics()
        {
            var stats = new ProductStatistics
            {
                TotalProducts = await ProductQueries.Active(db).CountAsync(),
                AvailableProducts = await ProductQueries.Active(db).CountAsync(x => x.IsAvailable),
                ProductsWithoutCards = await ProductQueries.Active(db).CountAsync(x => !x.WarrantyCards.Any()),
                RegisteredCards = await WarrantyCardQueries.All(db).CountAsync(x => x.IsRegistered),
                UnregisteredCards = await WarrantyCardQueries.All(db).CountAsync(x => !x.IsRegistered),
                TotalRegisteredPoints = await db.CardRegistrations
                    .SumAsync(x => (int?)x.WarrantyCard.Product.ProductPoint) ?? 0
            };
            return stats;
        }
    }
}
