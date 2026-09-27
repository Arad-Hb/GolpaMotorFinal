using DataAccess.Mappers;
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
            var product = await db.Products.FirstOrDefaultAsync(x => x.ProductID == productID && !x.IsDeleted);

            if (product == null)
                return null;

            return ProductMapper.ToAddEditModel(product);
        }

        public async Task<List<ProductListItem>> GetAll()
        {
            return await db.Products.Where(x => !x.IsDeleted)
                .Select(ProductMapper.ToListItem)
                .ToListAsync();
        }

        public async Task<ProductDetailsModel?> GetDetails(long productID)
        {
            return await db.Products
                .Where(x => x.ProductID == productID && !x.IsDeleted)
                .Select(ProductMapper.ToDetails)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> Exists(long id)
        {
            return await db.Products.AnyAsync(x => x.ProductID == id && !x.IsDeleted);
        }

        public async Task<ProductListComplexModel> Search(ProductSearchModel searchModel)
        {
            var result = new ProductListComplexModel();

            // 1. شروع Query
            var query = db.Products.AsQueryable();

            // 2. Soft Delete (خیلی مهم - همیشه اعمال شود)
            query = query.Where(x => !x.IsDeleted);

            // 3. فیلتر ProductID (اگر ارسال شده)
            if (searchModel.ProductID > 0)
            {
                query = query.Where(p => p.ProductID == searchModel.ProductID);
            }

            // 4. فیلتر نام محصول
            if (!string.IsNullOrWhiteSpace(searchModel.ProductName))
            {
                query = query.Where(p => p.ProductName.Contains(searchModel.ProductName));
            }

            // 5. اگر خواستی فیلتر وضعیت فعال/غیرفعال
            if (searchModel.IsAvailable.HasValue)
            {
                query = query.Where(p => p.IsAvailable == searchModel.IsAvailable.Value);
            }
            if (searchModel.PointsFrom.HasValue)
            {
                query = query.Where(p => p.ProductPoint >= searchModel.PointsFrom.Value);
            }
            if (searchModel.PointsTo.HasValue)
            {
                query = query.Where(p => p.ProductPoint <= searchModel.PointsTo.Value);
            }
            if (searchModel.RegisteredFrom.HasValue)
            {
                query = query.Where(p => p.WarrantyCards.Count(w => w.IsRegistered) >= searchModel.RegisteredFrom.Value);
            }
            if (searchModel.RegisteredTo.HasValue)
            {
                query = query.Where(p => p.WarrantyCards.Count(w => w.IsRegistered) <= searchModel.RegisteredTo.Value);
            }
            if (searchModel.RemainingFrom.HasValue)
            {
                query = query.Where(p => p.WarrantyCards.Count(w => !w.IsRegistered) >= searchModel.RemainingFrom.Value);
            }
            if (searchModel.RemainingTo.HasValue)
            {
                query = query.Where(p => p.WarrantyCards.Count(w => !w.IsRegistered) <= searchModel.RemainingTo.Value);
            }

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
                TotalProducts = await db.Products.CountAsync(x => !x.IsDeleted),
                AvailableProducts = await db.Products.CountAsync(x => !x.IsDeleted && x.IsAvailable),
                ProductsWithoutCards = await db.Products.CountAsync(x => !x.IsDeleted && !x.WarrantyCards.Any()),
                RegisteredCards = await db.WarrantyCards.CountAsync(x => x.IsRegistered),
                UnregisteredCards = await db.WarrantyCards.CountAsync(x => !x.IsRegistered),
                TotalRegisteredPoints = await db.CardRegistrations
                    .SumAsync(x => (int?)x.WarrantyCard.Product.ProductPoint) ?? 0
            };
            return stats;
        }
    }
}
