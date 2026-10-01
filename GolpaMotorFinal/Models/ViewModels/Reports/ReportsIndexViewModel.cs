using DomainModel.ViewModels.Product;
using GolpaMotorFinal.Models.ViewModels.CRUD;

namespace GolpaMotorFinal.Models.ViewModels.Reports
{
    public sealed class ReportsIndexViewModel
    {
        public string OpenTab { get; set; } = "";

        public int CurrentJalaliYear { get; set; }

        public IReadOnlyList<ReportTabViewModel> Tabs { get; set; }= Array.Empty<ReportTabViewModel>();

        public CrudIndexViewModel Page { get; set; } = new();
    }
}
