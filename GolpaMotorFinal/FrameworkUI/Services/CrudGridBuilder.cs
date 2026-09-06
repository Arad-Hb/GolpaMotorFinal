using GolpaMotorFinal.Models.ViewModels.CRUD;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public class CrudGridBuilder<T>
    {
        public CrudGridViewModel Grid { get; } = new();

        //public CrudGridBuilder<T> AddRows(GridColumn column)
        //{
        //    Grid.Columns.Add(column);
        //    return this;
        //}
        //public CrudGridBuilder<T> AddColumn(GridColumn column)
        //{
        //    Grid.Columns.Add(column);
        //    return this;
        //}

        //public CrudGridBuilder<T> AddAction(GridAction action)
        //{
        //    Grid.Actions.Add(action);
        //    return this;
        //}

        public CrudGridViewModel Build()
        {
            return Grid;
        }
    }
}
