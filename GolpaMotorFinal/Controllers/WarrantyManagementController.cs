using Application.Services;
using DomainModel.ViewModels.Warranty;
using Framework.Common;
using Framework.Common.Extensions;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.ProductManagement;
using GolpaMotorFinal.Models.ViewModels.WarrantyManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GolpaMotorFinal.Controllers
{
    public class WarrantyManagementController : Controller
    {
        private readonly IWarrantyExcelService excelService;
        private readonly IWarrantyService warrantyService;
        private readonly IProductService products;
        private readonly LookupLists lookups;

        public WarrantyManagementController(
               IWarrantyExcelService excelService,
               IWarrantyService warrantyService,
               IProductService products,
               LookupLists lookups)
        {
            this.excelService = excelService;
            this.warrantyService = warrantyService;
            this.products = products;
            this.lookups = lookups;
        }

        private async Task<RegisterationCardViewModel> EmptyRegisterForm()
        {
            var vm = new RegisterationCardViewModel
            {
                CustomerTypes = await lookups.CustomerTypeItems(),
                op = new OperationResult("WarrantyRegistration")
            };
            if (TempData["SuccessMessage"] is string ok)
                vm.op.ToSuccess(ok);
            else if (TempData["ErrorMessage"] is string err)
                vm.op.ToFailed(err);
            return vm;
        }

        private static string ResolveTab(string? tab)
        {
            return tab switch
            {
                "cards" or "excel" or "generate" or "register" => tab,
                _ => "register"
            };
        }

        private async Task<WarrantyCardsPageViewModel> BuildIndex(string tab, RegisterationCardViewModel? form = null)
        {
            form ??= new RegisterationCardViewModel
            {
                CustomerTypes = await lookups.CustomerTypeItems(),
                op = new OperationResult("WarrantyRegistration")
            };
            if (form.CustomerTypes == null || !form.CustomerTypes.Any())
                form.CustomerTypes = await lookups.CustomerTypeItems();
            form.op ??= new OperationResult("WarrantyRegistration");

            var productList = await products.GetAll();
            return new WarrantyCardsPageViewModel
            {
                Stats = await products.GetStatistics(),
                Filter = new WarrantyCardFilterBarViewModel { Products = productList },
                RegistrationCard = form,
                OpenTab = ResolveTab(tab)
            };
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string tab = "register")
        {
            return View(await BuildIndex(tab));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            return View(await EmptyRegisterForm());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Register(RegisterationCardViewModel request)
            => FinishRegister(request, fromAdmin: false);

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> RegisterForCustomer(RegisterationCardViewModel request)
            => FinishRegister(request, fromAdmin: true);

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> List(WarrantyCardSearchModel sm)
        {
            sm ??= new WarrantyCardSearchModel();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            sm.RegisteredFrom = sm.RegisteredFromJalali.ToGregorianDate();
            sm.RegisteredTo = sm.RegisteredToJalali.ToGregorianDate();
            var page = await warrantyService.SearchCards(sm);
            var grid = AdminListGrids.BuildWarrantyCardGrid(page.Items);
            CrudGridPager.Attach(
                grid,
                "warrantyCardsGrid",
                page.PageIndex,
                page.PageCount,
                page.RecordCount,
                FilterUrl.Combine("/WarrantyManagement/List", new
                {
                    sm.SearchTerm,
                    sm.ProductID,
                    sm.IsRegistered,
                    sm.ValidityPreset,
                    sm.RemainingDaysFrom,
                    sm.RemainingDaysTo,
                    sm.RegisteredFromJalali,
                    sm.RegisteredToJalali
                }));
            return ViewComponent("CrudGrid", new { model = grid });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excel(UploadWarrantyExcelViewModel model)
        {
            if (model.ProductID <= 0 || model.ExcelFile == null)
            {
                TempData["ErrorMessage"] = "محصول و فایل اکسل الزامی است.";
                return RedirectToAction(nameof(Index), new { tab = "excel" });
            }

            try
            {
                var result = await excelService.ImportExcel(model.ProductID, model.ExcelFile);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index), new { tab = "cards" });
                }

                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index), new { tab = "excel" });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index), new { tab = "excel" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(long productId, int count, int validityMonths = 12)
        {
            if (productId <= 0 || count <= 0)
            {
                TempData["ErrorMessage"] = "محصول و تعداد معتبر نیست.";
                return RedirectToAction(nameof(Index), new { tab = "generate" });
            }

            var result = await warrantyService.GenerateCodes(productId, count, validityMonths);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index), new { tab = "cards" });
            }

            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index), new { tab = "generate" });
        }

        private async Task<IActionResult> FinishRegister(RegisterationCardViewModel request, bool fromAdmin)
        {
            request.op ??= new OperationResult("WarrantyRegistration");

            if (!ModelState.IsValid)
            {
                request.op.ToFailed("اطلاعات وارد شده در فرم معتبر نیست.");
                ModelState.AddModelError("", "اطلاعات وارد شده در فرم معتبر نیست.");
                return await ShowRegisterForm(request, fromAdmin);
            }

            var result = await warrantyService.RegisterCards(new RegisterCardsRequest
            {
                CustomerPhoneNumber = request.CustomerPhoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CustomerTypeId = request.CustomerTypeId,
                Codes = request.ScratchedCode ?? new List<string>(),
                RateLimitKey = GetRateLimitKey()
            });

            if (!result.Success)
            {
                request.RateLimitRetryAfterSeconds = result.RetryAfterSeconds;
                request.op.ToFailed(result.Message);
                ModelState.AddModelError("", result.Message);
                foreach (var line in result.FailedLines)
                    ModelState.AddModelError("", line);
                return await ShowRegisterForm(request, fromAdmin);
            }

            TempData["SuccessMessage"] = result.Message;
            if (result.FailedLines.Count > 0)
                TempData["FailedCards"] = string.Join(" | ", result.FailedLines);

            return fromAdmin
                ? RedirectToAction(nameof(Index), new { tab = "cards" })
                : RedirectToAction(nameof(Register));
        }

        private async Task<IActionResult> ShowRegisterForm(RegisterationCardViewModel request, bool fromAdmin)
        {
            request.op ??= new OperationResult("WarrantyRegistration");
            request.CustomerTypes = await lookups.CustomerTypeItems();
            return View(fromAdmin ? "Index" : nameof(Register), fromAdmin ? await BuildIndex("register", request) : request);
        }

        private string GetRateLimitKey()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
                return "warranty-reg:" + userId;

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return "warranty-reg:" + ip;
        }
    }
}
