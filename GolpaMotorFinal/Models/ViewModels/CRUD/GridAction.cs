namespace GolpaMotorFinal.Models.ViewModels.CRUD
{
    public class GridAction
    {
        public string? Id { get; set; }
        public string? ActionText { get; set; }
        public string? Icon { get; set; }
        public string? Controller { get; set; } = string.Empty;
        public GridActionType ActionType { get; set; }
        public string? Area { get; set; }
        public string? Url { get; set; }
        public string CssClass { get; set; } = "table table-hover";
        public bool OpenModal { get; set; } = true;
        public bool EnableRefresh { get; set; } = true;
        public bool Ajax { get; set; } = true;
    }

}
