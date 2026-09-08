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
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace GolpaMotorFinal.Controllers
{
    public class WarrantyManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ICardRegistrationRepository repo;
        private readonly IWarrantyExcelService excelService;
        private readonly IWarrantyCardRepository warrantyCards;
        private readonly IProductRepository products;
        private readonly IRewardRequestRepository rewardRequests;
        private readonly IMemoryCache cache;
        private const int MaxCardsPerRequest = 10;
        private const int RegisterAttemptWindowSeconds = 60;

        public WarrantyManagementController(
               ICardRegistrationRepository repo,
               UserManager<ApplicationUser> userManager,
               RoleManager<IdentityRole> roleManager,
               IWarrantyExcelService excelService,
               IWarrantyCardRepository warrantyCards,
               IProductRepository products,
               IRewardRequestRepository rewardRequests,
               IMemoryCache cache)
        {
            this.repo = repo;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.excelService = excelService;
            this.warrantyCards = warrantyCards;
            this.products = products;
            this.rewardRequests = rewardRequests;
            this.cache = cache;
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
            return View("Register", request);
        }

        private async Task EnsureRoleAsync(string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        public async Task<IActionResult> Index(long? productId, bool? isRegistered, string? tab)
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction(nameof(Register));

            var vm = await BuildAdminIndex(productId, isRegistered);
            vm.OpenTab = ResolveOpenTab(tab, vm.LastImport != null);
            return View(vm);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register()
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
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Register(RegisterationCardViewModel request)
            => CompleteRegistration(request, fromAdmin: false);

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> RegisterForCustomer(RegisterationCardViewModel request)
            => CompleteRegistration(request, fromAdmin: true);

        private static string ResolveOpenTab(string? tab, bool hasImport)
        {
            return tab switch
            {
                "register" => "register",
                "excel" => "excel",
                "generate" => "generate",
                "cards" => "cards",
                _ => hasImport ? "excel" : "cards"
            };
        }

        private async Task<WarrantyAdminIndexViewModel> BuildAdminIndex(long? productId, bool? isRegistered, RegisterationCardViewModel? form = null)
        {
            form ??= new RegisterationCardViewModel();
            EnsureOperation(form);
            form.CustomerTypes = await BindCustomerTypes();

            if (TempData["SuccessMessage"] is string success)
                form.op!.ToSuccess(success);
            else if (TempData["ErrorMessage"] is string error)
                form.op!.ToFailed(error);

            var vm = new WarrantyAdminIndexViewModel
            {
                ProductID = productId,
                IsRegistered = isRegistered,
                Products = await products.GetAll(),
                Cards = await warrantyCards.SearchAsync(productId, isRegistered),
                Stats = await products.GetStatistics(),
                RegistrationCard = form
            };

            if (TempData["ImportMessage"] is string importMsg)
            {
                vm.LastImport = new WarrantyExcelImportResult
                {
                    Success = TempData["ImportSuccess"] as bool? ?? false,
                    Message = importMsg,
                    Inserted = TempData["ImportInserted"] as int? ?? 0,
                    Duplicate = TempData["ImportDuplicate"] as int? ?? 0,
                    Empty = TempData["ImportEmpty"] as int? ?? 0
                };
            }

            return vm;
        }

        private async Task<IActionResult> FailRegistration(RegisterationCardViewModel request, bool fromAdmin)
        {
            if (!fromAdmin)
                return await WarrantyForm(request);

            var vm = await BuildAdminIndex(null, null, request);
            vm.OpenTab = "register";
            return View("Index", vm);
        }

        private string GetRegistrationRateLimitKey()
        {
            var userId = userManager.GetUserId(User);
            if (!string.IsNullOrWhiteSpace(userId))
                return $"warranty-reg:{userId}";

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return $"warranty-reg:{ip}";
        }

        private bool TryConsumeRegistrationAttempt(out int retryAfterSeconds)
        {
            var key = GetRegistrationRateLimitKey();
            var now = DateTime.UtcNow;

            if (cache.TryGetValue(key, out DateTime lastAttemptUtc))
            {
                var elapsed = (int)(now - lastAttemptUtc).TotalSeconds;
                var remaining = RegisterAttemptWindowSeconds - elapsed;
                if (remaining > 0)
                {
                    retryAfterSeconds = remaining;
                    return false;
                }
            }

            cache.Set(key, now, TimeSpan.FromSeconds(RegisterAttemptWindowSeconds));
            retryAfterSeconds = 0;
            return true;
        }

        private async Task<IActionResult> CompleteRegistration(RegisterationCardViewModel request, bool fromAdmin)
        {
            var op = EnsureOperation(request);

            if (!TryConsumeRegistrationAttempt(out var retryAfterSeconds))
            {
                request.RateLimitRetryAfterSeconds = retryAfterSeconds;
                var message = $"لطفاً {retryAfterSeconds} ثانیه صبر کنید و سپس دوباره تلاش کنید.";
                op.ToFailed(message);
                ModelState.AddModelError("", message);
                return await FailRegistration(request, fromAdmin);
            }

            if (!ModelState.IsValid)
            {
                op.ToFailed("اطلاعات وارد شده در فرم معتبر نیست.");
                ModelState.AddModelError("", "اطلاعات وارد شده در فرم معتبر نیست.");
                return await FailRegistration(request, fromAdmin);
            }

            if (request.ScratchedCode == null || request.ScratchedCode.Count == 0)
            {
                op.ToFailed("اطلاعات کارت‌ها نامعتبر است.");
                ModelState.AddModelError("", "اطلاعات کارت‌ها نامعتبر است.");
                return await FailRegistration(request, fromAdmin);
            }

            if (request.ScratchedCode.Count > MaxCardsPerRequest)
            {
                op.ToFailed("حداکثر ۱۰ کارت گارانتی در هر درخواست قابل ثبت است.");
                ModelState.AddModelError("", "حداکثر ۱۰ کارت گارانتی در هر درخواست قابل ثبت است.");
                return await FailRegistration(request, fromAdmin);
            }

            var validCards = new List<WarrantyCard>();
            var invalidCards = new List<string>();
            var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < request.ScratchedCode.Count; i++)
            {
                var scratchedCode = request.ScratchedCode[i]?.Trim();

                if (string.IsNullOrWhiteSpace(scratchedCode))
                {
                    invalidCards.Add($"ردیف {i + 1}: رمز وارد نشده است.");
                    continue;
                }

                if (!seenCodes.Add(scratchedCode))
                {
                    invalidCards.Add($"ردیف {i + 1}: این رمز تکراری است.");
                    continue;
                }

                var (card, isAmbiguous) = await repo.GetByScratchedCodeAsync(scratchedCode);

                if (isAmbiguous || card == null)
                {
                    invalidCards.Add($"ردیف {i + 1}: رمز وارد شده معتبر نیست.");
                    continue;
                }

                if (card.Product == null)
                {
                    invalidCards.Add($"ردیف {i + 1}: محصول مرتبط با این کارت یافت نشد.");
                    continue;
                }

                var alreadyRegistered = await repo.IsRegisteredAsync(card.WarrantyCardID);

                if (alreadyRegistered)
                {
                    invalidCards.Add($"ردیف {i + 1}: این کارت قبلاً ثبت شده است.");
                    continue;
                }

                validCards.Add(card);
            }

            if (!validCards.Any())
            {
                op.ToFailed("اطلاعات وارد شده همه کارت‌ها نامعتبر است.");
                foreach (var error in invalidCards)
                    ModelState.AddModelError("", error);

                return await FailRegistration(request, fromAdmin);
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
                    return await FailRegistration(request, fromAdmin);
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
            await rewardRequests.RefreshEligibility(user.Id);

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

            return fromAdmin
                ? RedirectToAction(nameof(Index), new { tab = "register" })
                : RedirectToAction(nameof(Register));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExcel(UploadWarrantyExcelViewModel model)
        {
            if (model.ProductID <= 0 || model.ExcelFile == null)
            {
                TempData["ImportSuccess"] = false;
                TempData["ImportMessage"] = "محصول و فایل اکسل الزامی است.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await excelService.ImportExcel(model.ProductID, model.ExcelFile);
                TempData["ImportSuccess"] = result.Success;
                TempData["ImportMessage"] = result.Message;
                TempData["ImportInserted"] = result.Inserted;
                TempData["ImportDuplicate"] = result.Duplicate;
                TempData["ImportEmpty"] = result.Empty;
            }
            catch (Exception ex)
            {
                TempData["ImportSuccess"] = false;
                TempData["ImportMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateCodes(long productId, int count)
        {
            if (productId <= 0 || count <= 0)
            {
                TempData["ImportSuccess"] = false;
                TempData["ImportMessage"] = "محصول و تعداد معتبر نیست.";
                return RedirectToAction(nameof(Index));
            }

            if (count > 200)
                count = 200;

            var created = 0;
            for (var i = 0; i < count; i++)
            {
                string serial;
                do
                {
                    serial = WarrantyCodeGenerator.Serial();
                } while (await warrantyCards.SerialExistsAsync(serial));

                await warrantyCards.AddAsync(new WarrantyCard
                {
                    ProductID = productId,
                    SerialNumber = serial,
                    ScratchedCode = WarrantyCodeGenerator.ScratchedCode(),
                    IsRegistered = false,
                    ValidityMonths = 12
                });
                created++;
            }

            await warrantyCards.SaveAsync();
            TempData["ImportSuccess"] = true;
            TempData["ImportMessage"] = $"{created} کد گارانتی تولید شد.";
            return RedirectToAction(nameof(Index), new { productId });
        }
    }
}
