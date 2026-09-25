using Application.Services;
using DomainModel.ViewModels.Warranty;
using Framework.Common;
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

        public async Task<OperationResult> ImportExcel(long productId, IFormFile file)
        {
            var op = new OperationResult("WarrantyExcel");

            if (file == null || file.Length == 0)
                return op.ToFailed("فایل انتخاب نشده است");

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (ext != ".xlsx")
                return op.ToFailed("فقط فایل اکسل با پسوند xlsx پذیرفته می‌شود.");

            var items = new List<WarrantyCardImportItem>();

            using (var package = new ExcelPackage(file.OpenReadStream()))
            {
                var worksheet = package.Workbook.Worksheets[0];
                if (worksheet?.Dimension == null)
                    return op.ToFailed("فایل اکسل خالی است.");

                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var serial = ToEnglishDigits(worksheet.Cells[row, 1].Text);
                    var code = ToEnglishDigits(worksheet.Cells[row, 2].Text);
                    var monthsText = ToEnglishDigits(worksheet.Cells[row, 3].Text);

                    int? months = null;
                    if (!string.IsNullOrWhiteSpace(monthsText))
                    {
                        if (int.TryParse(monthsText, out var parsed))
                            months = parsed;
                        else
                            months = 0;
                    }

                    items.Add(new WarrantyCardImportItem
                    {
                        SerialNumber = serial,
                        ScratchedCode = code,
                        ValidityMonths = months
                    });
                }
            }

            var imported = await warrantyService.ImportCards(productId, items);
            return imported.Success
                ? op.ToSuccess(imported.Message)
                : op.ToFailed(imported.Message);
        }

        private static string ToEnglishDigits(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var chars = value.Trim().ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (chars[i] >= '۰' && chars[i] <= '۹')
                    chars[i] = (char)('0' + (chars[i] - '۰'));
                else if (chars[i] >= '٠' && chars[i] <= '٩')
                    chars[i] = (char)('0' + (chars[i] - '٠'));
            }

            return new string(chars);
        }
    }
}
