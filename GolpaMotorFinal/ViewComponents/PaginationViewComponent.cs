using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    public class PaginationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PaginationViewModel model)
        {
            if (model.PageSize <= 0)
                model.PageSize = PaginationViewModel.DefaultPageSize;

            return View(model);
        }
    }
}
