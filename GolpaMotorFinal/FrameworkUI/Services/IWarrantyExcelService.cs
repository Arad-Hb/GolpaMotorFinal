using Framework.Common;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public interface IWarrantyExcelService
    {
        Task<OperationResult> ImportExcel(long productId, IFormFile file);
    }
}
