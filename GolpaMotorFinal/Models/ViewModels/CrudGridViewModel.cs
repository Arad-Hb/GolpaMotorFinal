namespace GolpaMotorFinal.Models.ViewModels
{
    public class CrudGridViewModel
    {
        public string Id { get; set; } = "crudGrid";

        public string Controller { get; set; } = string.Empty;

        public string Action { get; set; } = "List";

        public string? Area { get; set; }

        public string CssClass { get; set; } = "table table-hover";

        public bool EnableRefresh { get; set; } = true;

        public bool Ajax { get; set; } = true;

        public Dictionary<string, string> RouteValues { get; set; } = new();
    }
}
