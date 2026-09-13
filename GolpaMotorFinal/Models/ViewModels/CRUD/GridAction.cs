namespace GolpaMotorFinal.Models.ViewModels.CRUD
{
    public class GridAction
    {
        public string? Id { get; set; }
        public string IdName { get; set; } = "userID";
        public string? ActionText { get; set; }
        public string? Icon { get; set; }
        public string? Controller { get; set; } = string.Empty;
        public GridActionType ActionType { get; set; }
        public string? Area { get; set; }
        public string? Url { get; set; }
        public string CssClass { get; set; } = "btn btn-sm btn-outline-secondary";
        public bool OpenModal { get; set; } = true;
        public bool EnableRefresh { get; set; } = true;
        public bool Ajax { get; set; } = true;
        public bool IsDelete { get; set; }
        public bool Visible { get; set; } = true;
        public string? Size { get; set; }
        public string? GridId { get; set; }
        public string? RefreshUrl { get; set; }
        public string? RefreshTargetId { get; set; }
    }
}
