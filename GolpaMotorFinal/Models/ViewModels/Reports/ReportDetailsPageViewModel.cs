namespace GolpaMotorFinal.Models.ViewModels.Reports
{
    public class ReportDetailsPageViewModel
    {
        public string Title { get; set; } = "جزئیات گزارش";
        public string GridId { get; set; } = "ReportActivityGrid";
        public string FilterPartial { get; set; } = "_ReportActivityFilterBar";
        public ReportActivityFilterBarViewModel Filter { get; set; } = new();
    }
}
