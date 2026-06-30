using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    public class SearchBoxViewComponent:ViewComponent
    {
        public IViewComponentResult Invoke(SearchBoxViewModel model)
        {
            return View(model);
        }
    }
}


