namespace DomainModel.ViewModels.Warranty
{
    public class WarrantyCardImportItem
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string ScratchedCode { get; set; } = string.Empty;
        public int? ValidityMonths { get; set; }
    }
}
