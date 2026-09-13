using DataAccess.Services;
using DomainModel.ViewModels.User;
using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.Account;
using GolpaMotorFinal.Models.ViewModels.CRUD;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly IUserRepository repo;
        private readonly IUserService service;
        private readonly IRewardRequestRepository rewardRequests;

        public UserManagementController(
            IUserRepository repo,
            IUserService service,
            IRewardRequestRepository rewardRequests)
        {
            this.repo = repo;
            this.service = service;
            this.rewardRequests = rewardRequests;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.CustomerTypes = await repo.GetCustomerTypes();
            ViewBag.Provinces = await repo.GetProvinces();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List(UserSearchModel sm)
        {
            BindUserFilter(sm);
            var page = await service.GetListPage(sm);
            var grid = AdminListGrids.BuildUserGrid(page.Items);
            CrudGridPager.Attach(
                grid,
                "UserGrid",
                page.PageIndex,
                page.PageCount,
                page.RecordCount,
                FilterUrl.Combine("/UserManagement/List", new
                {
                    sm.SearchTerm,
                    sm.CustomerTypeID,
                    sm.ProvinceID,
                    sm.CityID,
                    sm.PointsFrom,
                    sm.PointsTo,
                    sm.IsEligibleForReward,
                    sm.HasReceivedReward,
                    sm.CardFromJalali,
                    sm.CardToJalali
                }));
            return ViewComponent("CrudGrid", new { model = grid });
        }

        private static void BindUserFilter(UserSearchModel sm)
        {
            sm ??= new UserSearchModel();
            sm.CardFrom = PersianDate.ParseOrNull(sm.CardFromJalali);
            sm.CardTo = PersianDate.ParseOrNull(sm.CardToJalali);
        }

        [HttpGet]
        public async Task<IActionResult> MergeAccounts(string userID, string? gridId = null, string? refreshUrl = null)
        {
            var currentUser =await service.GetUserMergeAccounts(userID);

            if (string.IsNullOrWhiteSpace(currentUser.UserID))
                return NotFound();

            ViewBag.GridId = string.IsNullOrWhiteSpace(gridId) ? "UserGrid" : gridId;
            ViewBag.RefreshUrl = string.IsNullOrWhiteSpace(refreshUrl) ? Url.Action("List", "UserManagement") : refreshUrl;

            var vm = new MergeAccountsComplexViewModel
            {
                CurrentUser = currentUser,
               
                Search = new SearchBoxViewModel
                {
                    Action = "SearchUserForMerge",
                    Controller = "UserManagement",
                    SearchParameterName = "Search.SearchTerm",
                    Placeholder = "شماره موبایل",
                    UseAjax = true,
                    UpdateTargetId = "mergeResultContainer",
                    ComponentId = "mergeAccountSearch"
                }
            };

            return PartialView("_MergeAccounts",vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MergeAccounts(MergeAccountsComplexViewModel model)
        {
            if (model?.CurrentUser == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "اطلاعات نامعتبر است." });

                return RedirectToAction(nameof(Index));
            }

            var result = await service.MergeUsers(model.CurrentUser);

            // If AJAX call, return JSON so client can refresh grid and close modal
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(result);
            }

            if (!result.Success)
            {
                TempData["MergeMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchUserForMerge(MergeAccountsComplexViewModel model)
        {
            var found = await service.GetMergeSearchResult(model.Search?.SearchTerm ?? string.Empty);
            model.SearchAttempted = true;
            if (string.IsNullOrWhiteSpace(found.UserID) || found.UserID == model.CurrentUser?.UserID)
                model.SearchedUser = null;
            else
                model.SearchedUser = found;

            return PartialView("_MergeResult", model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var provinces = await repo.GetProvinces();
            var customerTypes = await repo.GetCustomerTypes();
            var cities =new List<DomainModel.Models.City>();

            var form = new CrudFormViewModel
            {
                Title = "افزودن کاربر",
                Controller = "UserManagement",
                Action = "Create",
                Method="POST",
                Enctype= "multipart/form-data",
                SubmitButtonText = "افزودن",
                CloseOnSuccess=true,
                RefreshGrid=true,
                GridId = "UserGrid",
                RefreshGridUrl = "List"
            };
            var vm = new UserAddEditViewModel
            {
                CustomerTypes = new SelectList(customerTypes, "CustomerTypeID", "Title"),
                Provinces = new SelectList(provinces, "ProvinceID", "Name"),
                Cities = new SelectList(cities, "CityID", "Name"),
                CrudFormViewModel = form
            };

            return PartialView("_Create",vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserAddEditViewModel vm)
        {

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "اطلاعات وارد شده معتبر نیست."
                });
            }

            vm.ProfileImageUrl = "/images/imageUsers/noimage.jpg";
            var model = new UserAddEditModel
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                CustomerTypeID = vm.CustomerTypeID,
                ProvinceID = vm.ProvinceID,
                CityID = vm.CityID,
                Address = vm.Address,
                PostalCode = vm.PostalCode,
                IsActive = vm.IsActive,
                ProfileImageUrl = vm.ProfileImageUrl
            };

            return Json(await service.AddUser(model, vm.ProfileImage));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string userID)
        {
            var vm = await service.GetForEdit(userID);

            if (vm == null) return NotFound();

            return PartialView("_Edit", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Edit(UserAddEditViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.Remove(nameof(vm.Password));

            if (string.IsNullOrWhiteSpace(vm.Email))
                ModelState.Remove(nameof(vm.Email));

            if (!vm.ProvinceID.HasValue)
                ModelState.Remove(nameof(vm.ProvinceID));

            if (!vm.CityID.HasValue)
                ModelState.Remove(nameof(vm.CityID));

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات معتبر نیست", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            if (string.IsNullOrWhiteSpace(vm.UserID))
                return Json(new { success = false, message = "شناسه کاربر معتبر نیست" });

            var model = new UserAddEditModel
            {
                UserID = vm.UserID,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                CustomerTypeID = vm.CustomerTypeID,
                ProvinceID = vm.ProvinceID,
                CityID = vm.CityID,
                Address = vm.Address,
                PostalCode = vm.PostalCode,
                CreditCartNumber = vm.CreditCartNumber,
                IBAN = vm.IBAN,
                AccountNumber = vm.AccountNumber,
                IsActive = vm.IsActive,
                IsDeleted = vm.IsDeleted
            };

            return Json(await service.UpdateUser(model, vm.ProfileImage));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(string userID)
        {
            var result = await service.DeleteUser(userID);
            return Json(result);
        }

        [HttpGet]
        public async Task<JsonResult> GetCitiesByProvince(int provinceId)
        {
            var cities = await repo.GetCitiesByProvinceId(provinceId);
            if (cities == null || !cities.Any())
            {
                return Json(new { success = false, data = Array.Empty<object>(), message = "شهری یافت نشد" });
            }

            return Json(new
            {
                success = true,
                data = cities.Select(c => new { cityID = c.CityID, name = c.Name })
            });
        }

        [HttpGet]
        public async Task<IActionResult> Details(string userID)
        {
            var user = await repo.GetDetails(userID);

            if (user == null)
                return NotFound();


            return PartialView("_Details", user);
        }

        [HttpGet]
        public async Task<IActionResult> EligibleRewards(string userID, int rewardPage = 0, int historyPage = 0)
        {
            if (string.IsNullOrWhiteSpace(userID))
                return NotFound();

            await rewardRequests.RefreshEligibility(userID);

            var user = await repo.GetDetails(userID);
            if (user == null)
                return NotFound();

            var pageSize = PaginationViewModel.DefaultPageSize;
            var allItems = await rewardRequests.GetEligibleCatalogsForUser(user.UserID);
            var allRequests = await rewardRequests.GetUserRequests(user.UserID);
            var rewards = CrudGridPager.Slice(allItems, rewardPage, pageSize);
            var history = CrudGridPager.Slice(allRequests, historyPage, pageSize);

            var vm = new EligibleRewardsDialogViewModel
            {
                UserID = user.UserID,
                CustomerName = $"{user.FirstName} {user.LastName}".Trim(),
                PhoneNumber = user.PhoneNumber,
                TotalEarnedPoints = user.TotalEarnedPoints,
                TotalSettledPoints = user.TotalSettledPoints,
                RemainedPoints = await rewardRequests.GetAvailablePoints(user.UserID),
                TotalRegisteredCards = user.TotalRegisteredCards,
                IsEligibleForReward = user.IsEligibleForReward,
                HasReceivedReward = user.HasReceivedReward,
                Items = rewards.Items,
                RecentRequests = history.Items,
                RewardPage = rewards.PageIndex,
                RewardPageCount = rewards.PageCount,
                RewardRecordCount = rewards.RecordCount,
                HistoryPage = history.PageIndex,
                HistoryPageCount = history.PageCount,
                HistoryRecordCount = history.RecordCount
            };

            return PartialView("_EligibleRewards", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RequestReward(string userID, int rewardCatalogID)
        {
            if (string.IsNullOrWhiteSpace(userID))
                return Json(new { success = false, message = "کاربر نامعتبر است." });

            var result = await rewardRequests.CreateRequest(userID, rewardCatalogID);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
