using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    public class PaginationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PaginationViewModel model)
        {
            if (model.PageCount <= 1 && model.RecordCount <= model.PageSize)
                return Content("");

            return View(model);
        }
    }
}
