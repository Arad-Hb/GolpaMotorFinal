namespace GolpaMotorFinal.Models.ViewModels
{
    public class CrudIndexViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string GridId { get; set; } = "crudGrid";
        public string ViewComponent { get; set; } = string.Empty;
        public string CreateUrl { get; set; } = string.Empty;
        public string CreateTitle { get; set; } = "افزودن";
        public string CreateButtonText { get; set; } = "افزودن";
        public BreadcrumbViewModel? Breadcrumb { get; set; }
    }
}
