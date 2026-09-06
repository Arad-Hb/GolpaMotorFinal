using GolpaMotorFinal.Models.ViewModels.CRUD;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public interface ICrudGridConfiguration<T>
    {
        CrudGridViewModel Configure(IEnumerable<T> items);
    }
}
