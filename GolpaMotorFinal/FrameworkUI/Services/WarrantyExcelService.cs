using Application.Services;
using DomainModel.ViewModels.Warranty;
using OfficeOpenXml;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public class WarrantyExcelService : IWarrantyExcelService
    {
        private readonly IWarrantyService warrantyService;

        public WarrantyExcelService(IWarrantyService warrantyService)
        {
            this.warrantyService = warrantyService;
        }

        public async Task<WarrantyExcelImportResult> ImportExcel(long productId, IFormFile file)
        {
            var result = new WarrantyExcelImportResult();

            if (file == null || file.Length == 0)
            {
                result.Message = "فایل انتخاب نشده است";
                return result;
            }

            var items = new List<WarrantyCardImportItem>();

            using (var package = new ExcelPackage(file.OpenReadStream()))
            {
                var worksheet = package.Workbook.Worksheets[0];
                if (worksheet?.Dimension == null)
                {
                    result.Message = "فایل اکسل خالی است.";
                    return result;
                }

                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    items.Add(new WarrantyCardImportItem
                    {
                        SerialNumber = worksheet.Cells[row, 1].Text?.Trim() ?? string.Empty,
                        ScratchedCode = worksheet.Cells[row, 2].Text?.Trim() ?? string.Empty
                    });
                }
            }

            var imported = await warrantyService.ImportCards(productId, items);
            result.Success = imported.Success;
            result.Message = imported.Message;
            result.Inserted = imported.Inserted;
            result.Duplicate = imported.Duplicate;
            result.Empty = imported.Empty;
            return result;
        }
    }
}
