using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels;
using DomainModel.ViewModels.User;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Models.ViewModels.Account;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GolpaMotorFinal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IUserRepository userRepository;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IUserRepository userRepository)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.userRepository = userRepository;
        }
        private async Task<SelectList> BindProvince()
        {
            var provinces = await userRepository.GetProvinces();
            //provinces.Insert(0, new Province { ProvinceID = -1, Name = "انتخاب استان" });
            SelectList lst = new SelectList(provinces, "ProvinceID", "Name");
            return lst;
        }

        [HttpGet]
        public async Task<JsonResult> GetCitiesByProvince(int provinceId)
        {
            var cities = await userRepository.GetCitiesByProvinceId(provinceId);


            if (cities == null || !cities.Any())
            {
                return Json(new { success = false, data = new List<object>(), message = "شهری یافت نشد" });
            }

            return Json(new { success = true, data = cities });           
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            var vm = new LoginViewModel { ReturnUrl = returnUrl };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {

                    TempData["SuccessMessage"] = $"خوش آمدید {model.Email}";
                    return Redirect(model.ReturnUrl);
                }

                var user = await userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    if (await userManager.IsInRoleAsync(user, "Admin"))
                        return RedirectToAction("Index", "Admin");

                    if (await userManager.IsInRoleAsync(user, "Customer"))
                        return RedirectToAction("Index", "Customer");
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "ایمیل یا رمز عبور اشتباه است.");

            return View(model);
        }
            
        [HttpGet]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            var vm = new RegisterViewModel {
                Provinces = await BindProvince(),
                Cities = new List<SelectListItem>(),
                ReturnUrl = returnUrl 
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Provinces = await BindProvince();
                model.Cities = new List<SelectListItem>();
                return View(model);
            }

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
                    return RedirectToAction("Index", "Home");
                }

            //foreach (var error in result.Errors)
            //ModelState.AddModelError(string.Empty, error.Description);
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
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Index", "Home");

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Index", "Home");

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return View("ConfirmEmail");

            return View("Error");
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
            if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            //var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);
            //await _emailSender.SendEmailAsync(model.Email, "Reset Password", $"Please reset your password by <a href=\"{callbackUrl}\">clicking here</a>.");

            return RedirectToAction("ForgotPasswordConfirmation");
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
            var vm = new ResetPasswordViewModel { Token = token, Email = email };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Login));

            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            // If the user does not have an account, prompt to create one
            var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
            return View("ExternalLoginConfirmation", new ExternalLoginViewModel { Email = email, ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Login));

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Customer");
                result = await userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(model.ReturnUrl);
                }
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        [HttpGet]
        public IActionResult LoginWith2fa(string returnUrl = null)
        {
            var vm = new LoginWith2faViewModel { ReturnUrl = returnUrl };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return RedirectToAction(nameof(Login));

            var result = await signInManager.TwoFactorSignInAsync("Authenticator", model.TwoFactorCode, model.RememberMe, rememberClient: false);
            if (result.Succeeded) return RedirectToLocal(model.ReturnUrl);
            if (result.IsLockedOut) return View("Lockout");

            ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            return View(model);
        }

        [HttpGet]
        public IActionResult LoginWithRecoveryCode(string returnUrl = null)
        {
            var vm = new LoginWithRecoveryCodeViewModel { ReturnUrl = returnUrl };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(model.RecoveryCode);
            if (result.Succeeded) return RedirectToLocal(model.ReturnUrl);
            if (result.IsLockedOut) return View("Lockout");

            ModelState.AddModelError(string.Empty, "Invalid recovery code.");
            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        //پروفایل کاربر      
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Manage()
        {
            var user = await userManager.GetUserAsync(User);

            if (await userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "Admin");

            if (await userManager.IsInRoleAsync(user, "Employee"))
                return RedirectToAction("Index", "Employee");

            return RedirectToAction("Profile", "Account");
        }
    }
}
