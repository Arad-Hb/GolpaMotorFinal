using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Helpers
{
    public static class AlertExtensions
    {
        public static void SetSuccessAlert(this Controller controller, string message)
        {
            controller.TempData["Success"] = true;
            controller.TempData["Message"] = message;
        }

        public static void SetErrorAlert(this Controller controller, string message)
        {
            controller.TempData["Success"] = false;
            controller.TempData["Message"] = message;
        }
    }
}
