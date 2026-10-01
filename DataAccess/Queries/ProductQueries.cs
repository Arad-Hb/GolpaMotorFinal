using DomainModel.Models;
using DomainModel.ViewModels.Product;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Queries
{
    public static class ProductQueries
    {
        public static IQueryable<Product> Active(GolpaMotorDbContext db)
            => db.Products.AsNoTracking().Where(x => !x.IsDeleted);

        public static IQueryable<Product> ApplySearch(IQueryable<Product> query,ProductSearchModel searchModel)
        {
            if (searchModel.ProductID > 0)
                query = query.Where(p => p.ProductID == searchModel.ProductID);

            if (!string.IsNullOrWhiteSpace(searchModel.ProductName))
                query = query.Where(p => p.ProductName.Contains(searchModel.ProductName));

            if (searchModel.IsAvailable.HasValue)
                query = query.Where(p => p.IsAvailable == searchModel.IsAvailable.Value);

            if (searchModel.PointsFrom.HasValue)
                query = query.Where(p => p.ProductPoint >= searchModel.PointsFrom.Value);

            if (searchModel.PointsTo.HasValue)
                query = query.Where(p => p.ProductPoint <= searchModel.PointsTo.Value);

            if (searchModel.RegisteredFrom.HasValue)
                query = query.Where(p => p.WarrantyCards.Count(w => w.IsRegistered) >= searchModel.RegisteredFrom.Value);

            if (searchModel.RegisteredTo.HasValue)
                query = query.Where(p => p.WarrantyCards.Count(w => w.IsRegistered) <= searchModel.RegisteredTo.Value);

            if (searchModel.RemainingFrom.HasValue)
                query = query.Where(p => p.WarrantyCards.Count(w => !w.IsRegistered) >= searchModel.RemainingFrom.Value);

            if (searchModel.RemainingTo.HasValue)
                query = query.Where(p => p.WarrantyCards.Count(w => !w.IsRegistered) <= searchModel.RemainingTo.Value);

            return query;
        }
    }
}
