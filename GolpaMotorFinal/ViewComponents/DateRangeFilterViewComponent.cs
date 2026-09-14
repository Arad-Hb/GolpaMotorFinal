using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    public class DateRangeFilterViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(DateRangeFilterViewModel model)
        {
            return View(model);
        }
    }
}
