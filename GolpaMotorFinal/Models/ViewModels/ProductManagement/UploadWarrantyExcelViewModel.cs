using DomainModel.ViewModels.Product;

namespace GolpaMotorFinal.Models.ViewModels.ProductManagement
{
    public class UploadWarrantyExcelViewModel
    {
        public long ProductID { get; set; }

        public IFormFile ExcelFile { get; set; }
        public List<ProductListItem> Products { get; set; } = new();
    }
}
