using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        // داشبورد اصلی ادمین
        public IActionResult Index()
        {
            return View();
        }
    }
}