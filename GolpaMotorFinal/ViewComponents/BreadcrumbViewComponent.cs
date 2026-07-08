using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    public class BreadcrumbViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(BreadcrumbViewModel? model)
        {
            if (model == null || model.Items.Count == 0)
                return Content("");

            return View(model);
        }
    }
}
