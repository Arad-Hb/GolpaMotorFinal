using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.ViewComponents
{
    public class AdminPageHeaderViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(AdminPageHeaderViewModel model)
        {
            model ??= new AdminPageHeaderViewModel();
            if (model.Breadcrumb == null || model.Breadcrumb.Items.Count == 0)
                model.Breadcrumb = BuildTrail(model);

            return View(model);
        }

        private BreadcrumbViewModel BuildTrail(AdminPageHeaderViewModel model)
        {
            var current = string.IsNullOrWhiteSpace(model.Current) ? model.Title : model.Current;
            var items = new List<BreadcrumbItemViewModel>();

            if (!IsDashboard(current) || model.Parents.Count > 0)
            {
                items.Add(new BreadcrumbItemViewModel
                {
                    Title = "داشبورد",
                    Url = Url.Action("Index", "Admin")
                });
            }

            foreach (var parent in model.Parents)
            {
                items.Add(new BreadcrumbItemViewModel
                {
                    Title = parent.Title,
                    Url = parent.Url
                });
            }

            if (!string.IsNullOrWhiteSpace(current))
            {
                items.Add(new BreadcrumbItemViewModel
                {
                    Title = current,
                    Active = true
                });
            }

            return new BreadcrumbViewModel { Items = items };
        }

        private static bool IsDashboard(string? title)
            => string.Equals(title?.Trim(), "داشبورد", StringComparison.Ordinal);
    }
}
