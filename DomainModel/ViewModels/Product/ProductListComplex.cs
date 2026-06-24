using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.Product
{
    public class ProductListComplex
    {
        public List<ProductListItem> productList {  get; set; }
        public ProductSearchModel sm { get; set; }
    }
}
