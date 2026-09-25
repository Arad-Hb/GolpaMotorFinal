using DomainModel.ViewModels.Product;

namespace GolpaMotorFinal.Models.ViewModels.WarrantyManagement
{
    public class WarrantyCardFilterBarViewModel
    {
        public List<ProductListItem> Products { get; set; } = new();
        public string? SearchTerm { get; set; }
        public long? ProductID { get; set; }
        public bool? IsRegistered { get; set; }
        public string? ValidityPreset { get; set; }
        public int? RemainingDaysFrom { get; set; }
        public int? RemainingDaysTo { get; set; }
        public string? RegisteredFromJalali { get; set; }
        public string? RegisteredToJalali { get; set; }
    }

    public class WarrantyCardsPageViewModel
    {
        public ProductStatistics? Stats { get; set; }
        public WarrantyCardFilterBarViewModel Filter { get; set; } = new();
        public RegisterationCardViewModel RegistrationCard { get; set; } = new();
        public string OpenTab { get; set; } = "register";
        public int GenerateCount { get; set; } = 5;
        public int ValidityMonths { get; set; } = 12;
    }
}
