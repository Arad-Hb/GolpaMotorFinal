using DataAccess.Services;
using DomainModel.Models;
using OfficeOpenXml;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public class WarrantyExcelService : IWarrantyExcelService
    {
        private readonly IWarrantyCardRepository repo;

        public WarrantyExcelService(IWarrantyCardRepository repo)
        {
            this.repo = repo;
        }

        public async Task<WarrantyExcelImportResult> ImportExcel(long productId, IFormFile file)
        {
            var result = new WarrantyExcelImportResult();

            if (file == null || file.Length == 0)
            {
                result.Message = "فایل انتخاب نشده است";
                return result;
            }

            var existing = await repo.GetSerialsAsync();
            var batchSerials = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var list = new List<WarrantyCard>();

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
                    var serial = worksheet.Cells[row, 1].Text?.Trim();
                    var code = worksheet.Cells[row, 2].Text?.Trim();

                    if (string.IsNullOrWhiteSpace(serial) && string.IsNullOrWhiteSpace(code))
                    {
                        result.Empty++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(serial) || string.IsNullOrWhiteSpace(code))
                    {
                        result.Empty++;
                        continue;
                    }

                    if (existing.Contains(serial) || batchSerials.Contains(serial))
                    {
                        result.Duplicate++;
                        continue;
                    }

                    batchSerials.Add(serial);
                    list.Add(new WarrantyCard
                    {
                        ProductID = productId,
                        SerialNumber = serial,
                        ScratchedCode = code,
                        IsRegistered = false,
                        ValidityMonths = 12
                    });
                }
            }

            if (list.Count > 0)
            {
                repo.AddRange(list);
                await repo.SaveAsync();
            }

            result.Inserted = list.Count;
            result.Success = true;
            result.Message =
                $"{result.Inserted} کارت درج شد، {result.Duplicate} تکراری و {result.Empty} ردیف خالی نادیده گرفته شد.";
            return result;
        }
    }
}
