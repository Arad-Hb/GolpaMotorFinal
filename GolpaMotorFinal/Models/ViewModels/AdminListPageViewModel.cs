namespace GolpaMotorFinal.Models.ViewModels
{
    public class AdminPageHeaderViewModel
    {
        public string Title { get; set; } = string.Empty;
        public BreadcrumbViewModel? Breadcrumb { get; set; }
    }

    public class AdminListPageViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Heading { get; set; }
        public string GridId { get; set; } = "crudGrid";
        public string? FilterPartial { get; set; }
        public BreadcrumbViewModel? Breadcrumb { get; set; }
    }
}
