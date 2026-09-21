using DomainModel.ViewModels.Warranty;
using Framework.Common;

namespace Application.Services
{
    public interface IWarrantyService
    {
        Task<(List<WarrantyCardListItem> Items, int PageIndex, int PageCount, int RecordCount)> SearchCards(WarrantyCardSearchModel sm);
        Task<OperationResult> GenerateCodes(long productId, int count, int validityMonths = 12);
        Task<WarrantyImportResult> ImportCards(long productId, IReadOnlyList<WarrantyCardImportItem> items);
    }
}
