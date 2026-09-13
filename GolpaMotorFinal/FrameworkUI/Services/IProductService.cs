using DomainModel.ViewModels.Product;
using Framework.Common;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public interface IProductService
    {
        Task<OperationResult> AddProduct(ProductAddEditModel prod, IFormFile? imageFile);
        Task<OperationResult> UpdateProduct(ProductAddEditModel prod, IFormFile? imageFile);
        Task<OperationResult> DeleteProduct(long productID);
        Task<ProductAddEditModel?> GetForEdit(long productID);
        Task<OperationResult> RemovePicture(long productID);
        Task<ProductStatistics> GetStatistics();
        Task<ProductListComplexModel> Search(ProductSearchModel sm);
        Task<ProductDetailsModel?> GetDetails(long productID);
    }
}
