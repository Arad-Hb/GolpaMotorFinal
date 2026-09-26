using DomainModel.Models;
using DomainModel.ViewModels.Product;

namespace GolpaMotorFinal.Models.ViewModels
{
    public class UserFilterBarViewModel
    {
        public string Url { get; set; } = "/UserManagement/List";
        public string Target { get; set; } = "#UserGrid";
        public IEnumerable<CustomerType> CustomerTypes { get; set; } = Enumerable.Empty<CustomerType>();
        public IEnumerable<Province> Provinces { get; set; } = Enumerable.Empty<Province>();
    }

    public class ProductReportFilterBarViewModel
    {
        public int YearNow { get; set; }
    }

    public class ProductWarrantyReportFilterBarViewModel
    {
        public IEnumerable<ProductListItem> Products { get; set; } = Enumerable.Empty<ProductListItem>();
    }

    public class ReportActivityFilterBarViewModel
    {
        public string Url { get; set; } = "/Reports/Activities";
        public string Target { get; set; } = "#ReportActivityGrid";
        public IEnumerable<ProductListItem> Products { get; set; } = Enumerable.Empty<ProductListItem>();
        public long? ProductID { get; set; }
        public string? UserID { get; set; }
        public long? WarrantyCardID { get; set; }
        public int? RewardRequestID { get; set; }
        public int? RewardCatalogID { get; set; }
    }
}
