using GolpaMotorFinal.Models.ViewModels.CRUD;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public interface IGridConfigurationFactory
    {
        CrudGridViewModel Build<T>(IEnumerable<T> items);
    }
}
