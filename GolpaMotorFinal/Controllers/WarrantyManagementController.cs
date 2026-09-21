using Application.Services;
using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Warranty;
using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Models.ViewModels.ProductManagement;
using GolpaMotorFinal.Models.ViewModels.WarrantyManagement;
using GolpaMotorFinal.Models.ViewModels;
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
        private readonly IWarrantyService warrantyService;
        private readonly IProductService products;
        private readonly IRewardService rewards;
        private readonly IMemoryCache cache;
        private const int MaxCardsPerRequest = 10;
        private const int RegisterAttemptWindowSeconds = 60;

        public WarrantyManagementController(
               ICardRegistrationRepository repo,
               UserManager<ApplicationUser> userManager,
               RoleManager<IdentityRole> roleManager,
               IWarrantyExcelService excelService,
               IWarrantyService warrantyService,
               IProductService products,
               IRewardService rewards,
               IMemoryCache cache)
        {
            this.repo = repo;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.excelService = excelService;
            this.warrantyService = warrantyService;
            this.products = products;
            this.rewards = rewards;
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

        public async Task<IActionResult> Index(long? productId, bool? isRegistered, string? tab, int pageIndex = 0)
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction(nameof(Register));

            var vm = await BuildAdminIndex(productId, isRegistered, pageIndex: pageIndex);
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

        private async Task<WarrantyAdminIndexViewModel> BuildAdminIndex(long? productId, bool? isRegistered, RegisterationCardViewModel? form = null, int pageIndex = 0)
        {
            form ??= new RegisterationCardViewModel();
            EnsureOperation(form);
            form.CustomerTypes = await BindCustomerTypes();

            if (TempData["SuccessMessage"] is string success)
                form.op!.ToSuccess(success);
            else if (TempData["ErrorMessage"] is string error)
                form.op!.ToFailed(error);

            var pageSize = PaginationViewModel.DefaultPageSize;
            var filter = new WarrantyCardSearchModel
            {
                ProductID = productId,
                IsRegistered = isRegistered,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            var search = await warrantyService.SearchCards(filter);
            var vm = new WarrantyAdminIndexViewModel
            {
                ProductID = productId,
                IsRegistered = isRegistered,
                Products = await products.GetAll(),
                Cards = search.Items,
                PageIndex = search.PageIndex,
                PageCount = search.PageCount,
                RecordCount = search.RecordCount,
                Stats = await products.GetStatistics(),
                RegistrationCard = form
            };

            if (TempData["ImportMessage"] is string importMsg)
            {
                vm.LastImport = new WarrantyExcelImportResult
                {
                    Success = TempData["ImportSuccess"]?.ToString() == "1",
                    Message = importMsg,
                    Inserted = int.TryParse(TempData["ImportInserted"]?.ToString(), out var inserted) ? inserted : 0,
                    Duplicate = int.TryParse(TempData["ImportDuplicate"]?.ToString(), out var duplicate) ? duplicate : 0,
                    Empty = int.TryParse(TempData["ImportEmpty"]?.ToString(), out var empty) ? empty : 0,
                    Invalid = int.TryParse(TempData["ImportInvalid"]?.ToString(), out var invalid) ? invalid : 0
                };
            }

            return vm;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> List(WarrantyCardSearchModel sm)
        {
            sm ??= new WarrantyCardSearchModel();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            sm.RegisteredFrom = PersianDate.ParseOrNull(sm.RegisteredFromJalali);
            sm.RegisteredTo = PersianDate.ParseOrNull(sm.RegisteredToJalali);
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
            await rewards.RefreshEligibility(user.Id);

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
                SetImportTempData(false, "محصول و فایل اکسل الزامی است.");
                return RedirectToAction(nameof(Index), new { tab = "excel" });
            }

            try
            {
                var result = await excelService.ImportExcel(model.ProductID, model.ExcelFile);
                SetImportTempData(result.Success, result.Message, result.Inserted, result.Duplicate, result.Empty, result.Invalid);
                return RedirectToAction(nameof(Index), new
                {
                    tab = result.Success ? "cards" : "excel",
                    productId = model.ProductID
                });
            }
            catch (Exception ex)
            {
                SetImportTempData(false, ex.Message);
                return RedirectToAction(nameof(Index), new { tab = "excel", productId = model.ProductID });
            }
        }

        private void SetImportTempData(bool success, string message, int inserted = 0, int duplicate = 0, int empty = 0, int invalid = 0)
        {
            TempData["ImportSuccess"] = success ? "1" : "0";
            TempData["ImportMessage"] = message;
            TempData["ImportInserted"] = inserted.ToString();
            TempData["ImportDuplicate"] = duplicate.ToString();
            TempData["ImportEmpty"] = empty.ToString();
            TempData["ImportInvalid"] = invalid.ToString();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateCodes(long productId, int count,int validityMonths=12)
        {
            if (productId <= 0 || count <= 0)
            {
                SetImportTempData(false, "محصول و تعداد معتبر نیست.");
                return RedirectToAction(nameof(Index), new { tab = "generate" });
            }

            var result = await warrantyService.GenerateCodes(productId, count, validityMonths);
            SetImportTempData(result.Success, result.Message);
            return RedirectToAction(nameof(Index), new { productId, tab = "cards" });
        }
    }
}
