namespace GolpaMotorFinal.Models.ViewModels.CRUD
{
    public class GridColumn
    {
        public string? Header { get; set; }

        public string? Text { get; set; }
        public string EmptyText { get; set; } = "-";

        public GridColumnType Type { get; set; } = GridColumnType.Text;

        public string? CssClass { get; set; }

        public string? BadgeClass { get; set; }
        public object? Value { get; set; }

        public string? ImageUrl { get; set; }

        public string? Icon { get; set; }
        public string? TrueIcon { get; set; }
        public string? FalseIcon { get; set; }

        public string? ButtonText { get; set; }

        public string? ButtonClass { get; set; }

        public string? ButtonUrl { get; set; }
    }
}
