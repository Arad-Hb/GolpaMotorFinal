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

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (ext != ".xlsx")
            {
                result.Message = "فقط فایل اکسل با پسوند xlsx پذیرفته می‌شود.";
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
            result.Success = imported.Success;
            result.Message = imported.Message;
            result.Inserted = imported.Inserted;
            result.Duplicate = imported.Duplicate;
            result.Empty = imported.Empty;
            result.Invalid = imported.Invalid;
            return result;
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
