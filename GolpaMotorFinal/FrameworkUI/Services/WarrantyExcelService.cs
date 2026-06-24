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

        public void ImportExcel(long productId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("فایل انتخاب نشده است");

            using (var package = new ExcelPackage(file.OpenReadStream()))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                var list = new List<WarrantyCard>();

                for (int row = 2; row <= rowCount; row++)
                {
                    list.Add(new WarrantyCard
                    {
                        ProductID = productId,
                        SerialNumber = worksheet.Cells[row, 1].Text,
                        ScratchedCode = worksheet.Cells[row, 2].Text,
                        IsRegistered = false,
                        ValidityMonths = 12
                    });
                }

                repo.AddRange(list);
                repo.Save();
            }
        }
    }
}
