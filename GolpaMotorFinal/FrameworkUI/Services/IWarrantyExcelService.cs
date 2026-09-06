namespace GolpaMotorFinal.FrameworkUI.Services
{
    public class WarrantyExcelImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Inserted { get; set; }
        public int Duplicate { get; set; }
        public int Empty { get; set; }
    }

    public interface IWarrantyExcelService
    {
        Task<WarrantyExcelImportResult> ImportExcel(long productId, IFormFile file);
    }
}
