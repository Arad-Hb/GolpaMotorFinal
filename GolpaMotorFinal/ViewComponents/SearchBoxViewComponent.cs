using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    public class SearchBoxViewComponent:ViewComponent
    {
        public IViewComponentResult Invoke(string placeholderText, string actionName, string controllerName)
        {
            var model = new SearchBoxViewModel
            {
                Placeholder = placeholderText,
                Action = actionName,
                Controller = controllerName
            };
            return View(model);
        }
    }
}
