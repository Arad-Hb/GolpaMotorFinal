using DomainModel.ViewModels.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.Product
{
    public class ProductListComplexModel
    {
        public List<ProductListItem> productList { get; set; }
        public ProductSearchModel sm { get; set; }
        public ProductListComplexModel()
        {
            productList = new List<ProductListItem>();
        }
    }
}
