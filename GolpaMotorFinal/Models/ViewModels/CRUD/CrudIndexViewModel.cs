namespace GolpaMotorFinal.Models.ViewModels.CRUD
{
    public class CrudIndexViewModel
    {
        public string GridId { get; set; } = "crudGrid";
        public string CreateUrl { get; set; } = string.Empty;
        public string CreateTitle { get; set; } = "افزودن";
        public string CreateButtonText { get; set; } = "افزودن";
        public string ListHeading { get; set; } = "لیست";
        public string? FilterPartial { get; set; }
        public bool ShowCreate => !string.IsNullOrWhiteSpace(CreateUrl);
    }
}
