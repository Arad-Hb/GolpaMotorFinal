namespace DomainModel.ViewModels.Warranty
{
    public class WarrantyCardImportItem
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string ScratchedCode { get; set; } = string.Empty;
        public int? ValidityMonths { get; set; }
    }

    public class WarrantyImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Inserted { get; set; }
        public int Duplicate { get; set; }
        public int Empty { get; set; }
        public int Invalid { get; set; }
    }
}
