using DomainModel.Models;

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
}
