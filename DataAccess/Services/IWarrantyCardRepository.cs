using DomainModel.Models;
using DomainModel.ViewModels.Product;
using DomainModel.ViewModels.Warranty;

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
        Task<(List<WarrantyCardListItem> Items, int Total)> SearchAsync(WarrantyCardSearchModel search);
    }

}
