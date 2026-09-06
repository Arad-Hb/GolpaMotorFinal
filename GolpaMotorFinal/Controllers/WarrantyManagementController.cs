using DataAccess.Services;
using DomainModel.Models;
using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Models.ViewModels.ProductManagement;
using GolpaMotorFinal.Models.ViewModels.WarrantyManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace GolpaMotorFinal.Controllers
{
    public class WarrantyManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ICardRegistrationRepository repo;
        private readonly IWarrantyExcelService excelService;

        public WarrantyManagementController(
               ICardRegistrationRepository repo,
               UserManager<ApplicationUser> userManager,
               RoleManager<IdentityRole> roleManager,
               IWarrantyExcelService excelService)
        {
            this.repo = repo;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.excelService = excelService;
        }

        private async Task<IEnumerable<SelectListItem>> BindCustomerTypes()
        {
            var types = await repo.GetCustomerTypes();
            return types.Select(t => new SelectListItem
            {
                Value = t.CustomerTypeID.ToString(),
                Text = t.Title
            });
        }

        private static OperationResult EnsureOperation(RegisterationCardViewModel request)
        {
            request.op ??= new OperationResult("WarrantyRegistration");
            return request.op;
        }

        private async Task<IActionResult> WarrantyForm(RegisterationCardViewModel request)
        {
            EnsureOperation(request);
            request.CustomerTypes = await BindCustomerTypes();
            return View("Index", request);
        }

        private async Task EnsureRoleAsync(string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        public async Task<IActionResult> Index()
        {
            var vm = new RegisterationCardViewModel
            {
                CustomerTypes = await BindCustomerTypes(),
                op = new OperationResult("WarrantyRegistration")
            };

            if (TempData["SuccessMessage"] is string success)
                vm.op.ToSuccess(success);
            else if (TempData["ErrorMessage"] is string error)
                vm.op.ToFailed(error);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterationCardViewModel request)
        {
            var op = EnsureOperation(request);

            if (!ModelState.IsValid)
            {
                op.ToFailed("اطلاعات وارد شده در فرم معتبر نیست.");
                ModelState.AddModelError("", "اطلاعات وارد شده در فرم معتبر نیست.");
                return await WarrantyForm(request);
            }

            if (request.SerialNumber == null ||
                request.ScratchedCode == null ||
                request.SerialNumber.Count == 0 ||
                request.SerialNumber.Count != request.ScratchedCode.Count)
            {
                op.ToFailed("اطلاعات کارت‌ها نامعتبر است.");
                ModelState.AddModelError("", "اطلاعات کارت‌ها نامعتبر است.");
                return await WarrantyForm(request);
            }

            var validCards = new List<WarrantyCard>();
            var invalidCards = new List<string>();

            for (int i = 0; i < request.SerialNumber.Count; i++)
            {
                var serial = request.SerialNumber[i]?.Trim();
                var scratchedCode = request.ScratchedCode[i]?.Trim();

                if (string.IsNullOrWhiteSpace(serial) ||
                    string.IsNullOrWhiteSpace(scratchedCode))
                {
                    invalidCards.Add($"ردیف {i + 1}: شماره سریال یا رمز وارد نشده است.");
                    continue;
                }

                var card = await repo.GetBySerialAsync(serial, scratchedCode);

                if (card == null)
                {
                    invalidCards.Add(
                        $"ردیف {i + 1}: سریال {serial} و رمز وارد شده معتبر نیستند.");
                    continue;
                }

                if (card.Product == null)
                {
                    invalidCards.Add(
                        $"ردیف {i + 1}: محصول مرتبط با سریال {serial} یافت نشد.");
                    continue;
                }

                var alreadyRegistered = await repo.IsRegisteredAsync(card.WarrantyCardID);

                if (alreadyRegistered)
                {
                    invalidCards.Add(
                        $"ردیف {i + 1}: کارت با سریال {serial} قبلاً ثبت شده است.");
                    continue;
                }

                validCards.Add(card);
            }

            if (!validCards.Any())
            {
                op.ToFailed("اطلاعات وارد شده همه کارت‌ها نامعتبر است.");
                foreach (var error in invalidCards)
                    ModelState.AddModelError("", error);

                return await WarrantyForm(request);
            }

            var user = await userManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == request.CustomerPhoneNumber);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = request.CustomerPhoneNumber,
                    PhoneNumber = request.CustomerPhoneNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    EmailConfirmed = true,
                    IsActive = true,
                    IsConfirmedCode = true
                };

                var tempPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12)) + "Aa1";
                var createResult = await userManager.CreateAsync(user, tempPassword);

                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    op.ToFailed("ایجاد کاربر ناموفق بود.");
                    return await WarrantyForm(request);
                }
            }

            await EnsureRoleAsync("Customer");

            if (!await userManager.IsInRoleAsync(user, "Customer"))
                await userManager.AddToRoleAsync(user, "Customer");

            if (request.CustomerTypeId.HasValue)
            {
                var alreadyLinked = await repo.IsCardAlreadyRegisteredByUserAsync(
                    request.CustomerTypeId.Value, user.Id);

                if (!alreadyLinked)
                {
                    await repo.AddUserCustomerType(new UserCustomerType
                    {
                        UserID = user.Id,
                        CustomerTypeID = request.CustomerTypeId.Value
                    });
                }
            }

            foreach (var card in validCards)
            {
                await repo.AddRegistration(new CardRegistration
                {
                    WarrantyCardID = card.WarrantyCardID,
                    UserID = user.Id,
                    SerialNumber = card.SerialNumber,
                    ScratchedCode = card.ScratchedCode,
                    CustomerPhoneNumber = request.CustomerPhoneNumber,
                    CreatedAt = DateTime.UtcNow
                });

                card.IsRegistered = true;

                await repo.AddTransaction(new PointTransaction
                {
                    UserID = user.Id,
                    PointsAmount = card.Product.ProductPoint,
                    PointTransactionDate = DateTime.UtcNow,
                    Description = $"ثبت کارت گارانتی {card.SerialNumber}"
                });
            }

            await repo.SaveChangesAsync();

            var successCount = validCards.Count;
            var failCount = invalidCards.Count;

            if (failCount > 0)
            {
                TempData["SuccessMessage"] =
                    $"{successCount} کارت ثبت شد و {failCount} کارت نامعتبر بود.";
                TempData["FailedCards"] = string.Join(" | ", invalidCards);
            }
            else
            {
                TempData["SuccessMessage"] = $"{successCount} کارت با موفقیت ثبت شد.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult UploadExcel(UploadWarrantyExcelViewModel model)
        {
            try
            {
                if (model.ProductID <= 0 || model.ExcelFile == null)
                    return Json(new { success = false, message = "محصول و فایل اکسل الزامی است." });

                excelService.ImportExcel(model.ProductID, model.ExcelFile);
                return Json(new { success = true, message = "کارت‌های گارانتی با موفقیت وارد شدند." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
