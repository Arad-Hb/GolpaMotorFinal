using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Product;
using Framework.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess.Repositories
{
    public class ProductRepository : IProductRepository
    {

        private readonly GolpaMotorDbContext db;

        public ProductRepository(GolpaMotorDbContext db)
        {
            this.db = db;
        }
        private Product ToDbModel(ProductAddEditModel product)
        {
            return new Product
            {
                ProductName = product.ProductName,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                ProductPoint = product.ProductPoint,
                IsAvailable = product.IsAvailable,
                IsDeleted = false,
                //CreatedDate = DateTime.Now
            };
        }
        private ProductAddEditModel ToViewModel(Product product)
        {
            return new ProductAddEditModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                ProductPoint = product.ProductPoint,
                IsAvailable = product.IsAvailable
            };
        }

        public async Task<OperationResult> Add(ProductAddEditModel product)
        {
            var op = new OperationResult("Add Product");

            try
            {
                var p = ToDbModel(product);

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

                prod.ProductName = product.ProductName;
                prod.Description = product.Description;
                prod.ImageUrl = product.ImageUrl; //فقط FileName ذخیره می‌شود (نه path کامل)               
                prod.ProductPoint = product.ProductPoint;
                prod.IsAvailable = product.IsAvailable;

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

            return ToViewModel(product);
        }

        public async Task<List<ProductListItem>> GetAll()
        {
            return await db.Products.Where(x => !x.IsDeleted)
                .Select(x => new ProductListItem
                {
                    ProductID = x.ProductID,
                    ProductName = x.ProductName,
                    ImageUrl = x.ImageUrl ?? string.Empty,
                    ProductPoint = x.ProductPoint,
                    IsAvailable = x.IsAvailable
                })
                .ToListAsync();
        }

        public async Task<ProductDetailsModel?> GetDetails(long productID)
        {
            var product = await db.Products
                .Include(x => x.WarrantyCards)
                .FirstOrDefaultAsync(x => x.ProductID == productID && !x.IsDeleted);

            if (product == null)
                return null;

            return new ProductDetailsModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                ProductPoint = product.ProductPoint,
                IsAvailable = product.IsAvailable,
                WarrantyCount = product.WarrantyCards.Count
            };
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
                .Select(product => new ProductListItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ProductPoint = product.ProductPoint,
                    IsAvailable = product.IsAvailable,
                    ImageUrl = product.ImageUrl
                })
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
    }
}
