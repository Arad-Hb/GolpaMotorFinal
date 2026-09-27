using DomainModel.ViewModels.Product;
using Framework.Common;

namespace Application.Services
{
    public interface IProductService
    {
        Task<OperationResult> AddProduct(ProductAddEditModel prod);
        Task<OperationResult> UpdateProduct(ProductAddEditModel prod);
        Task<OperationResult> DeleteProduct(long productID);
        Task<OperationResult> RemovePicture(long productID);
        Task<ProductAddEditModel?> Get(long productID);
        Task<ProductDetailsModel?> GetDetails(long productID);
        Task<ProductStatistics> GetStatistics();
        Task<ProductListComplexModel> Search(ProductSearchModel sm);
        Task<List<ProductListItem>> GetAll();
    }
}
