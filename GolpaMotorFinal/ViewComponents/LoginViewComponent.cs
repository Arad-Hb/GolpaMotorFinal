using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    public class LoginViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }

}
