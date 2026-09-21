using DomainModel.ViewModels.Product;
using GolpaMotorFinal.Models.ViewModels.ProductManagement;

namespace GolpaMotorFinal.Mappers
{
    public static class ProductViewMapper
    {
        public static ProductAddEditModel ToAddEditModel(ProductAddEditViewModel vm)
        {
            return new ProductAddEditModel
            {
                ProductID = vm.ProductID,
                ProductName = vm.ProductName,
                Description = vm.Description,
                ProductPoint = vm.ProductPoint,
                IsAvailable = vm.IsAvailable,
                ImageUrl = vm.ExistingImageUrl
            };
        }

        public static ProductAddEditViewModel ToAddEditViewModel(ProductAddEditModel prod)
        {
            return new ProductAddEditViewModel
            {
                ProductID = prod.ProductID,
                ProductName = prod.ProductName,
                Description = prod.Description,
                ProductPoint = prod.ProductPoint,
                IsAvailable = prod.IsAvailable,
                ExistingImageUrl = prod.ImageUrl
            };
        }
    }
}
