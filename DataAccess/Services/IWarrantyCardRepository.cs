using DomainModel.Models;
using DomainModel.ViewModels.Product;

namespace DataAccess.Services
{
    public interface IWarrantyCardRepository
    {
        void AddRange(List<WarrantyCard> cards);
        void Save();
        Task SaveAsync();
        Task AddAsync(WarrantyCard card);
        Task<bool> SerialExistsAsync(string serialNumber);
        Task<HashSet<string>> GetSerialsAsync();
        Task<(List<WarrantyCardListItem> Items, int Total)> SearchAsync(long? productId, bool? isRegistered, int pageIndex = 0, int pageSize = 10);
    }

    public class WarrantyCardListItem
    {
        public long WarrantyCardID { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string ScratchedCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public bool IsRegistered { get; set; }
        public int ValidityMonths { get; set; }
    }
}
