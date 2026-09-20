namespace GolpaMotorFinal.Models.ViewModels
{
    public class AdminPageHeaderViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Current { get; set; }
        public List<BreadcrumbItemViewModel> Parents { get; set; } = new();
        public BreadcrumbViewModel? Breadcrumb { get; set; }

        public static AdminPageHeaderViewModel For(string title, string? current = null)
        {
            return new AdminPageHeaderViewModel
            {
                Title = title,
                Current = current
            };
        }
    }
}
