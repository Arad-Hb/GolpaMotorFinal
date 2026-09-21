using DomainModel.Models;
using DomainModel.ViewModels.Product;
using System.Linq.Expressions;

namespace DataAccess.Mappers
{
    public static class ProductMapper
    {
        public static Product ToEntity(ProductAddEditModel product)
        {
            return new Product
            {
                ProductName = product.ProductName,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                ProductPoint = product.ProductPoint,
                IsAvailable = product.IsAvailable,
                IsDeleted = false
            };
        }

        public static void Apply(Product product, ProductAddEditModel model)
        {
            product.ProductName = model.ProductName;
            product.Description = model.Description;
            product.ImageUrl = model.ImageUrl;
            product.ProductPoint = model.ProductPoint;
            product.IsAvailable = model.IsAvailable;
        }

        public static ProductAddEditModel ToAddEditModel(Product product)
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

        public static Expression<Func<Product, ProductListItem>> ToListItem =>
            x => new ProductListItem
            {
                ProductID = x.ProductID,
                ProductName = x.ProductName,
                ImageUrl = x.ImageUrl ?? string.Empty,
                ProductPoint = x.ProductPoint,
                IsAvailable = x.IsAvailable,
                RegisteredCardCount = x.WarrantyCards.Count(w => w.IsRegistered),
                UnregisteredCardCount = x.WarrantyCards.Count(w => !w.IsRegistered)
            };

        public static Expression<Func<Product, ProductDetailsModel>> ToDetails =>
            x => new ProductDetailsModel
            {
                ProductID = x.ProductID,
                ProductName = x.ProductName,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                ProductPoint = x.ProductPoint,
                IsAvailable = x.IsAvailable,
                WarrantyCount = x.WarrantyCards.Count
            };
    }
}
