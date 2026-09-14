using System.ComponentModel.DataAnnotations;

namespace DomainModel.ViewModels.Product
{
    public class ProductStatistics
    {
        public int TotalProducts { get; set; }
        public int AvailableProducts { get; set; }
        public int ProductsWithoutCards { get; set; }
        public int RegisteredCards { get; set; }
        public int UnregisteredCards { get; set; }
        public int TotalRegisteredPoints { get; set; }
    }
}
