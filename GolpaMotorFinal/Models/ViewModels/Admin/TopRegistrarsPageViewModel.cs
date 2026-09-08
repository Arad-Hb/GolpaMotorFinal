using DomainModel.ViewModels.Product;

namespace GolpaMotorFinal.Models.ViewModels.Admin
{
    public class TopRegistrarsPageViewModel
    {
        public List<NamedCountItem> Items { get; set; } = new();
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public int RecordCount { get; set; }
    }
}
