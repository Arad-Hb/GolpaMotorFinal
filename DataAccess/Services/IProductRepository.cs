using DomainModel.ViewModels.Product;
using Framework.Common;
using Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Services
{
    public interface IProductRepository 
    {
        Task<OperationResult> Add(ProductAddEditModel product);
        Task<OperationResult> Update(ProductAddEditModel product);
        Task<OperationResult> Delete(long productID);
        Task<ProductAddEditModel?> Get(long productID);
        Task<List<ProductListItem>> GetAll();
        Task<ProductDetailsModel?> GetDetails(long productID);
        Task<bool> Exists(long productID);
        Task<ProductListComplexModel> Search(ProductSearchModel sm);
        Task RemoveImage(long productID);        
    }
}
