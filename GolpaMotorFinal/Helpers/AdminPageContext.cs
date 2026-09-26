using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GolpaMotorFinal.Helpers
{
    public static class AdminPageContext
    {
        public static AdminPageHeaderViewModel For(ViewContext viewContext)
        {
            var controller = viewContext.RouteData.Values["controller"]?.ToString() ?? string.Empty;
            var action = viewContext.RouteData.Values["action"]?.ToString() ?? string.Empty;

            var (title, current) = Resolve(controller, action);
            return AdminPageHeaderViewModel.For(title, current);
        }

        private static (string Title, string Current) Resolve(string controller, string action)
        {
            if (controller.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                && action.Equals("Profile", StringComparison.OrdinalIgnoreCase))
                return ("پروفایل", "پروفایل");

            if (controller.Equals("Account", StringComparison.OrdinalIgnoreCase)
                && action.Equals("ChangePassword", StringComparison.OrdinalIgnoreCase))
                return ("تغییر رمز عبور", "تغییر رمز عبور");

            return controller.ToLowerInvariant() switch
            {
                "warrantymanagement" => ("مدیریت کارت‌های گارانتی", "گارانتی"),
                "productmanagement" => ("مدیریت محصولات", "محصولات"),
                "usermanagement" => ("مدیریت کاربران", "کاربران"),
                "rewardmanagement" => ("مدیریت پاداش", "پاداش"),
                "reports" => ("گزارشات", "گزارشات"),
                "settings" => ("تنظیمات", "تنظیمات"),
                _ => ("داشبورد", "داشبورد")
            };
        }
    }
}
