namespace GolpaMotorFinal.Models.ViewModels.CRUD
{
    public class GridRow
    {
        public string Key { get; set; }

        public List<GridColumn> Columns { get; set; } = new();
        public List<GridAction> Actions { get; set; } = new();
    }
}
