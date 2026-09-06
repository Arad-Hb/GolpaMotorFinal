using GolpaMotorFinal.Models.ViewModels.CRUD;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    public class CrudGridViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke<T>(CrudGridViewModel model)
        {
            return View(model);
        }

    }
}
