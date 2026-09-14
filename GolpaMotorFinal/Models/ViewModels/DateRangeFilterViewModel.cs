namespace GolpaMotorFinal.Models.ViewModels
{
    public class DateRangeFilterViewModel
    {
        public string FromName { get; set; } = "fromJalali";

        public string ToName { get; set; } = "toJalali";

        public string EmptyLabel { get; set; } = "انتخاب تاریخ";

        public string FromPlaceholder { get; set; } = "از تاریخ";

        public string ToPlaceholder { get; set; } = "تا تاریخ";

        public string? FromValue { get; set; }

        public string? ToValue { get; set; }
    }
}
