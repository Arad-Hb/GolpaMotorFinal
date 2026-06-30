using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    public class AllertViewComponent:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var success = TempData["Success"];
            var message = TempData["Message"];

            if (message == null)
                return Content("");

            ViewBag.Success = success;
            ViewBag.Message = message;

            return View();
        }
    }
}
