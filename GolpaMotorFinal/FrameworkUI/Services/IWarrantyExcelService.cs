namespace GolpaMotorFinal.FrameworkUI.Services
{
    public interface IWarrantyExcelService
    {
        void ImportExcel(long productId, IFormFile file);
    }
}
