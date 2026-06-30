using DataAccess.Services;
using DomainModel.Models;
using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Models.ViewModels.Account;
using GolpaMotorFinal.Models.ViewModels.WarrantyManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GolpaMotor.Controllers
{
    public class WarrantyManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ICardRegistrationRepository repo;

        public WarrantyManagementController(ICardRegistrationRepository repo,
               UserManager<ApplicationUser> userManager,
               RoleManager<IdentityRole> roleManager)
        {
            this.repo = repo;
            this.userManager = userManager;
            this.roleManager = roleManager;
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
        // صفحه فرم
        public async Task<IActionResult> Index(RegisterationCardViewModel? request)
        {
            var vm = new RegisterationCardViewModel
            {
                CustomerTypes = await BindCustomerTypes(),
                op = request?.op
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterationCardViewModel request)
        {
            var op = new OperationResult("WarrantyRegistration");
            if (!ModelState.IsValid)
            {
                request.op.ToFailed("اطلاعات وارد شده در فرم معتبر نیست.");
                ModelState.AddModelError("", "اطلاعات وارد شده در فرم معتبر نیست.");
                return View("Index",request);
            }

            if (request.SerialNumber == null ||
                request.ScratchedCode == null ||
                request.SerialNumber.Count == 0 ||
                request.SerialNumber.Count != request.ScratchedCode.Count)
            {
                request.op.ToFailed("اطلاعات کارت‌ها نامعتبر است.");
                ModelState.AddModelError("", "اطلاعات کارت‌ها نامعتبر است.");
                return View("Index", request);
            }

            var validCards = new List<WarrantyCard>();
            var invalidCards = new List<string>();

            // بررسی تک تک کارت‌ها
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

                var alreadyRegistered =await repo.IsRegisteredAsync(card.WarrantyCardID);

                if (alreadyRegistered)
                {
                    invalidCards.Add(
                        $"ردیف {i + 1}: کارت با سریال {serial} قبلاً ثبت شده است.");
                    continue;
                }

                validCards.Add(card);
            }

            // هیچ کارت معتبری پیدا نشد
            if (!validCards.Any())
            {
                foreach (var error in invalidCards)
                {
                    request.op.ToFailed("اطلاعات وارد شده همه کارتها نامعتبر است.");
                    ModelState.AddModelError("", error);
                }

                return View("Index", request);
            }

            // پیدا کردن کاربر
            var user = await userManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == request.CustomerPhoneNumber);

            // ایجاد کاربر در صورت عدم وجود
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

                var createResult = await userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View("Index", request);
                }

                await userManager.AddToRoleAsync(user, "Customer");
            }
            else
            {
                if (!await userManager.IsInRoleAsync(user, "Customer"))
                {
                    await userManager.AddToRoleAsync(user, "Customer");
                }
            }

            // ثبت نوع مشتری
            if (request.CustomerTypeId.HasValue)
            {
                var IsCardAlreadyRegisteredByUser = await repo.IsCardAlreadyRegisteredByUserAsync(
                    request.CustomerTypeId.Value, user.Id
                    );
                        

                if (!IsCardAlreadyRegisteredByUser)
                {
                    await repo.AddUserCustomerType(new UserCustomerType
                    {
                        UserID = user.Id,
                        CustomerTypeID = request.CustomerTypeId.Value
                    });
                }
            }

            // ثبت کارت‌های معتبر
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

            request.op.ToSuccess($"{successCount} کارت با موفقیت ثبت شد.");
            //TempData["Success"] =
            //    $"{successCount} کارت با موفقیت ثبت شد.";

            if (failCount > 0)
            {

                request.op.ToFailed($"{successCount} کارت ثبت شد و {failCount} کارت نامعتبر بود.");
                //TempData["Warning"] =
                //    $"{failCount} کارت ثبت نشد.";

                TempData["FailedCards"] =
                    string.Join("<br/>", invalidCards);
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
