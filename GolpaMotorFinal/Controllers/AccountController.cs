using DataAccess.Services;
using DomainModel.Models;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IUserRepository userRepository;
        private readonly ICardRegistrationRepository cardRepository;
        private readonly IFileManager fileManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUserRepository userRepository,
            ICardRegistrationRepository cardRepository,
            IFileManager fileManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.userRepository = userRepository;
            this.cardRepository = cardRepository;
            this.fileManager = fileManager;
        }

        [HttpGet]
        [AllowAnonymous]
        private async Task<SelectList> BindProvince()
        {
            var provinces = await userRepository.GetProvinces();
            return new SelectList(provinces, "ProvinceID", "Name");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetCitiesByProvince(int provinceId)
        {
            var cities = await userRepository.GetCitiesByProvinceId(provinceId);
            if (cities == null || !cities.Any())
                return Json(new { success = false, data = Array.Empty<object>(), message = "شهری یافت نشد" });

            return Json(new
            {
                success = true,
                data = cities.Select(c => new { cityID = c.CityID, name = c.Name })
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    TempData["SuccessMessage"] = $"خوش آمدید {model.Email}";
                    return Redirect(model.ReturnUrl);
                }

                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null && await userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Admin");

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "ایمیل یا رمز عبور اشتباه است.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            return View(new RegisterViewModel
            {
                Provinces = await BindProvince(),
                Cities = new List<SelectListItem>(),
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!model.ProvinceID.HasValue)
                ModelState.AddModelError("ProvinceID", "لطفا استان را انتخاب کنید");
            if (!model.CityID.HasValue)
                ModelState.AddModelError("CityID", "لطفا یک شهر انتخاب کنید.");

            if (!ModelState.IsValid)
            {
                model.Provinces = await BindProvince();
                model.Cities = new List<SelectListItem>();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ProvinceID = model.ProvinceID.Value,
                CityID = model.CityID.Value
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Customer");
                await signInManager.SignInAsync(user, false);
                TempData["SuccessMessage"] = "ثبت نام شما با موفقیت انجام شد. به گلپا موتور خوش آمدید.";
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                switch (error.Code)
                {
                    case "PasswordRequiresLower":
                        ModelState.AddModelError("", "رمز عبور باید حداقل یک حرف کوچک انگلیسی داشته باشد.");
                        break;
                    case "PasswordRequiresUpper":
                        ModelState.AddModelError("", "رمز عبور باید حداقل یک حرف بزرگ انگلیسی داشته باشد.");
                        break;
                    case "PasswordRequiresDigit":
                        ModelState.AddModelError("", "رمز عبور باید حداقل یک عدد داشته باشد.");
                        break;
                    case "DuplicateEmail":
                        ModelState.AddModelError("", "این ایمیل قبلاً ثبت شده است.");
                        break;
                    case "DuplicateUserName":
                        ModelState.AddModelError("", "این کاربر قبلاً ثبت شده است.");
                        break;
                    default:
                        ModelState.AddModelError("", error.Description);
                        break;
                }
            }

            model.Provinces = await BindProvince();
            model.Cities = new List<SelectListItem>();
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null && await userManager.IsEmailConfirmedAsync(user))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                TempData["ResetUrl"] = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);
            }

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token = null, string email = null)
        {
            if (token == null || email == null)
                return RedirectToAction("Index", "Home");
            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await signInManager.SignOutAsync();
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            TempData["SuccessMessage"] = "با موفقیت از حساب کاربری خارج شدید.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(Login));

            var changed = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changed.Succeeded)
            {
                foreach (var error in changed.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "رمز عبور با موفقیت تغییر کرد.";
            if (await userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Profile", "Admin");
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(Login));

            if (await userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Profile", "Admin");

            return View(await BuildProfileModel(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ManageViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(Login));

            if (await userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Profile", "Admin");

            user.FirstName = model.FirstName?.Trim();
            user.LastName = model.LastName?.Trim();
            user.Address = model.Address?.Trim();
            user.PostalCode = model.PostalCode?.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();

            if (model.ProfileImage != null)
            {
                var upload = await fileManager.UploadAsync(
                    model.ProfileImage,
                    5,
                    new[] { "jpg", "jpeg", "png" },
                    "images/imageUsers/uploads",
                    "images/imageUsers/thumbnails");
                if (!upload.Success)
                {
                    ModelState.AddModelError(string.Empty, upload.Message);
                    return View(await BuildProfileModel(user));
                }
                user.ProfileImageUrl = upload.FileUrl;
            }

            var update = await userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                foreach (var error in update.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(await BuildProfileModel(user));
            }

            await signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "پروفایل به‌روزرسانی شد.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult Manage()
        {
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public async Task<IActionResult> Cards(int pageIndex = 0)
        {
            var user = await userManager.GetUserAsync(User);//current loged in user
            if (user == null)
                return Unauthorized();

            return PartialView("_ManageCardsTable", await BuildProfileModel(user, pageIndex));
        }

        private async Task<ManageViewModel> BuildProfileModel(ApplicationUser user, int cardPage = 0)
        {
            var cards = await cardRepository.GetByUserAsync(user.Id);
            var mapped = cards.Select(c => new CustomerCardItem
            {
                SerialNumber = c.SerialNumber,
                ProductName = c.WarrantyCard?.Product?.ProductName ?? "-",
                CreatedAt = c.CreatedAt,
                Points = c.WarrantyCard?.Product?.ProductPoint ?? c.EarnedPionts
            }).ToList();

            var page = CrudGridPager.Slice(mapped, cardPage);
            return new ManageViewModel
            {
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                PostalCode = user.PostalCode,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                RemainedPoints = user.RemainedPoints ?? 0,
                TotalRegisteredCards = cards.Count,
                IsEligibleForReward = user.IsEligibleForReward,
                HasReceivedReward = user.HasReceivedReward,
                Cards = page.Items,
                CardPage = page.PageIndex,
                CardPageCount = page.PageCount,
                CardRecordCount = page.RecordCount
            };
        }
    }
}
