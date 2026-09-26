using Application.Services;
using DomainModel.ViewModels.Settings;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ILookupService lookups;

        public SettingsController(ILookupService lookups)
        {
            this.lookups = lookups;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> List(string? title, int pageIndex = 0)
        {
            var all = await lookups.GetCustomerTypes();
            if (!string.IsNullOrWhiteSpace(title))
            {
                var term = title.Trim();
                all = all.Where(x => x.Title.Contains(term)).ToList();
            }

            var page = CrudGridPager.Slice(all, pageIndex);
            var grid = AdminListGrids.BuildCustomerTypeGrid(page.Items);
            CrudGridPager.Attach(
                grid,
                "CustomerTypeGrid",
                page.PageIndex,
                page.PageCount,
                page.RecordCount,
                FilterUrl.Combine("/Settings/List", new { title }));
            return ViewComponent("CrudGrid", new { model = grid });
        }

        [HttpGet]
        public IActionResult Create()
            => PartialView("_Create", new CustomerTypeAddEditModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(CustomerTypeAddEditModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات معتبر نیست" });

            var result = await lookups.AddCustomerType(model);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await lookups.GetCustomerType(id);
            if (model == null)
                return NotFound();
            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Edit(CustomerTypeAddEditModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات معتبر نیست" });

            var result = await lookups.UpdateCustomerType(model);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(int id)
        {
            var result = await lookups.DeleteCustomerType(id);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
