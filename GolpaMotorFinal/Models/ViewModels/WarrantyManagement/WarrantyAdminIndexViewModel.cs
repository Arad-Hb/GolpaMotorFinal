using DataAccess.Services;
using DomainModel.ViewModels.Product;
using GolpaMotorFinal.FrameworkUI.Services;

namespace GolpaMotorFinal.Models.ViewModels.WarrantyManagement
{
    public class WarrantyAdminIndexViewModel
    {
        public List<WarrantyCardListItem> Cards { get; set; } = new();
        public List<ProductListItem> Products { get; set; } = new();
        public long? ProductID { get; set; }
        public bool? IsRegistered { get; set; }
        public int GenerateCount { get; set; } = 10;
        public ProductStatistics? Stats { get; set; }
        public WarrantyExcelImportResult? LastImport { get; set; }
    }
}
