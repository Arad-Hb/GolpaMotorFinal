using DataAccess.Services;
using DomainModel.ViewModels.Product;
using GolpaMotorFinal.FrameworkUI.Services;

namespace GolpaMotorFinal.Models.ViewModels.WarrantyManagement
{
    public class WarrantyAdminIndexViewModel
    {
        public List<WarrantyCardListItem> Cards { get; set; } = new();
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public int RecordCount { get; set; }
        public List<ProductListItem> Products { get; set; } = new();
        public long? ProductID { get; set; }
        public bool? IsRegistered { get; set; }
        public string? SearchTerm { get; set; }
        public string? ValidityPreset { get; set; }
        public int? RemainingDaysFrom { get; set; }
        public int? RemainingDaysTo { get; set; }
        public string? RegisteredFromJalali { get; set; }
        public string? RegisteredToJalali { get; set; }
        public int GenerateCount { get; set; } = 5;
        public ProductStatistics? Stats { get; set; }
        public WarrantyExcelImportResult? LastImport { get; set; }
        public RegisterationCardViewModel RegistrationCard { get; set; } = new();
        public string OpenTab { get; set; } = "cards";
    }
}
