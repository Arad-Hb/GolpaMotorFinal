using Application.Services;
using DataAccess.Mappers;
using DataAccess.Services;
using DomainModel.ViewModels.Warranty;
using Framework.Common;

namespace ApplicationService.Services
{
    public class WarrantyService : IWarrantyService
    {
        private readonly IWarrantyCardRepository cards;
        private readonly IProductRepository products;

        public WarrantyService(IWarrantyCardRepository cards, IProductRepository products)
        {
            this.cards = cards;
            this.products = products;
        }

        public async Task<(List<WarrantyCardListItem> Items, int PageIndex, int PageCount, int RecordCount)> SearchCards(WarrantyCardSearchModel sm)
        {
            sm ??= new WarrantyCardSearchModel();
            var search = await cards.SearchAsync(sm);
            var pageSize = sm.PageSize <= 0 ? 10 : sm.PageSize;
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

                await cards.AddAsync(WarrantyCardMapper.ToEntity(
                    productId, serial, WarrantyCodeGenerator.ScratchedCode(), validityMonths));
                created++;
            }

            await cards.SaveAsync();
            return op.ToSuccess($"{created} کد گارانتی تولید شد.");
        }

        public async Task<WarrantyImportResult> ImportCards(long productId, IReadOnlyList<WarrantyCardImportItem> items)
        {
            var result = new WarrantyImportResult();
            if (productId <= 0 || !await products.Exists(productId))
            {
                result.Message = "محصول معتبر نیست.";
                return result;
            }

            items ??= Array.Empty<WarrantyCardImportItem>();
            var existing = await cards.GetSerialsAsync();
            var batchSerials = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var list = new List<DomainModel.Models.WarrantyCard>();

            foreach (var item in items)
            {
                var serial = item.SerialNumber?.Trim();
                var code = item.ScratchedCode?.Trim();

                if (string.IsNullOrWhiteSpace(serial) && string.IsNullOrWhiteSpace(code) && !item.ValidityMonths.HasValue)
                {
                    result.Empty++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(serial) || string.IsNullOrWhiteSpace(code))
                {
                    result.Empty++;
                    continue;
                }

                var months = item.ValidityMonths ?? 12;
                if (months <= 0 || months > 60)
                {
                    result.Invalid++;
                    continue;
                }

                if (existing.Contains(serial) || batchSerials.Contains(serial))
                {
                    result.Duplicate++;
                    continue;
                }

                batchSerials.Add(serial);
                list.Add(WarrantyCardMapper.ToEntity(productId, serial, code, months));
            }

            if (list.Count > 0)
            {
                cards.AddRange(list);
                await cards.SaveAsync();
            }

            result.Inserted = list.Count;
            result.Success = true;
            result.Message =
                $"{result.Inserted} کارت درج شد، {result.Duplicate} تکراری، {result.Empty} خالی و {result.Invalid} نامعتبر نادیده گرفته شد.";
            return result;
        }
    }
}
