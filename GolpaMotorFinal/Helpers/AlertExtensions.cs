using Framework.Common;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Helpers
{
    public static class AlertExtensions
    {
        public static void ShowAlert(
            this Controller controller,
            OperationResult op)
        {
            controller.TempData["Success"] = op.Success;
            controller.TempData["Message"] = op.Message;
        }
    }
}
