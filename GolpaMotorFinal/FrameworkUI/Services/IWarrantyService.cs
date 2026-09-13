using DataAccess.Services;
using DomainModel.Models;
using Framework.Common;
using GolpaMotorFinal.Models.ViewModels;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public interface IWarrantyService
    {
        Task<(List<WarrantyCardListItem> Items, int PageIndex, int PageCount, int RecordCount)> SearchCards(WarrantyCardSearchModel sm);
        Task<OperationResult> GenerateCodes(long productId, int count, int validityMonths = 12);
    }

    public class WarrantyService : IWarrantyService
    {
        private readonly IWarrantyCardRepository cards;

        public WarrantyService(IWarrantyCardRepository cards)
        {
            this.cards = cards;
        }

        public async Task<(List<WarrantyCardListItem> Items, int PageIndex, int PageCount, int RecordCount)> SearchCards(WarrantyCardSearchModel sm)
        {
            sm ??= new WarrantyCardSearchModel();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var search = await cards.SearchAsync(sm);
            var pageSize = sm.PageSize <= 0 ? PaginationViewModel.DefaultPageSize : sm.PageSize;
            var pageCount = pageSize <= 0 ? 1 : (int)Math.Ceiling(search.Total / (double)pageSize);
            return (search.Items, sm.PageIndex, pageCount, search.Total);
        }

        public async Task<OperationResult> GenerateCodes(long productId, int count, int validityMonths = 12)
        {
            var op = new OperationResult("GenerateWarrantyCodes");
            if (productId <= 0 || count <= 0)
                return op.ToFailed("محصول و تعداد معتبر نیست.");
            if (count > 200)
                count = 200;

            var created = 0;
            for (var i = 0; i < count; i++)
            {
                string serial;
                do
                {
                    serial = WarrantyCodeGenerator.Serial();
                }
                while (await cards.SerialExistsAsync(serial));

                await cards.AddAsync(new WarrantyCard
                {
                    ProductID = productId,
                    SerialNumber = serial,
                    ScratchedCode = WarrantyCodeGenerator.ScratchedCode(),
                    IsRegistered = false,
                    ValidityMonths = validityMonths
                });
                created++;
            }

            await cards.SaveAsync();
            return op.ToSuccess($"{created} کد گارانتی تولید شد.");
        }
    }
}
