namespace GolpaMotorFinal.Models.ViewModels
{
    public class BreadcrumbItemViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool Active { get; set; }
    }

    public class BreadcrumbViewModel
    {
        public List<BreadcrumbItemViewModel> Items { get; set; } = new();
    }
}
