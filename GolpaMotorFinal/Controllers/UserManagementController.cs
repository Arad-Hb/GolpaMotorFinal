using Application.Services;
using DomainModel.ViewModels.User;
using Framework.Common;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Helpers;
using GolpaMotorFinal.Mappers;
using GolpaMotorFinal.Models;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.Account;
using GolpaMotorFinal.Models.ViewModels.CRUD;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly IUserService service;
        private readonly IFileManager fileManager;
        private readonly IRewardService rewards;

        public UserManagementController(
            IUserService service,
            IFileManager fileManager,
            IRewardService rewards)
        {
            this.service = service;
            this.fileManager = fileManager;
            this.rewards = rewards;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.CustomerTypes = await service.GetCustomerTypes();
            ViewBag.Provinces = await service.GetProvinces();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List(UserSearchModel sm)
        {
            BindUserFilter(sm);
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var result = await service.Search(sm);
            var page = result.sm ?? sm;
            var items = (result.userList ?? new List<UserListItem>()).Select(UserViewMapper.ToListItem).ToList();
            var grid = AdminListGrids.BuildUserGrid(items);
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
            var user = await service.GetDetails(userID);
            if (user == null || string.IsNullOrWhiteSpace(user.UserID))
                return NotFound();

            ViewBag.GridId = string.IsNullOrWhiteSpace(gridId) ? "UserGrid" : gridId;
            ViewBag.RefreshUrl = string.IsNullOrWhiteSpace(refreshUrl) ? Url.Action("List", "UserManagement") : refreshUrl;

            var vm = new MergeAccountsComplexViewModel
            {
                CurrentUser = UserViewMapper.ToMerge(user),
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

            return PartialView("_MergeAccounts", vm);
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

            var result = await service.MergeUsers(model.CurrentUser.UserID, model.CurrentUser.SelectedMergeUserID ?? string.Empty);

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
            var found = await service.GetUserDetail(model.Search?.SearchTerm ?? string.Empty);
            model.SearchAttempted = true;
            if (string.IsNullOrWhiteSpace(found.UserID) || found.UserID == model.CurrentUser?.UserID)
                model.SearchedUser = null;
            else
                model.SearchedUser = UserViewMapper.ToMerge(found);

            return PartialView("_MergeResult", model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var provinces = await service.GetProvinces();
            var customerTypes = await service.GetCustomerTypes();
            var cities = new List<DomainModel.Models.City>();

            var form = new CrudFormViewModel
            {
                Title = "افزودن کاربر",
                Controller = "UserManagement",
                Action = "Create",
                Method = "POST",
                Enctype = "multipart/form-data",
                SubmitButtonText = "افزودن",
                CloseOnSuccess = true,
                RefreshGrid = true,
                GridId = "UserGrid",
                RefreshGridUrl = "List"
            };

            var vm = UserViewMapper.ToAddEditViewModel(new UserAddEditModel(), customerTypes, provinces, cities, form);
            return PartialView("_Create", vm);
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

            var model = UserViewMapper.ToAddEditModel(vm);
            model.ProfileImageUrl = "/images/imageUsers/noimage.jpg";

            var uploaded = await TryUpload(vm.ProfileImage);
            if (uploaded is { Success: false })
                return Json(new OperationResult("AddUser").ToFailed(uploaded.Message));
            if (uploaded is { Success: true })
                model.ProfileImageUrl = uploaded.FileUrl;

            var result = await service.AddUser(model);
            if (!result.Success && uploaded is { Success: true })
                RemoveUserImage(uploaded.FileUrl);

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string userID)
        {
            var user = await service.Get(userID);
            if (user == null)
                return NotFound();

            var provinces = await service.GetProvinces();
            var customerTypes = await service.GetCustomerTypes();
            var cities = user.ProvinceID.HasValue
                ? await service.GetCitiesByProvinceId(user.ProvinceID.Value)
                : new List<DomainModel.Models.City>();

            var form = new CrudFormViewModel
            {
                Title = "ویرایش کاربر",
                Controller = "UserManagement",
                Action = "Edit",
                Method = "POST",
                Enctype = "multipart/form-data",
                SubmitButtonText = "ثبت نهایی",
                CloseOnSuccess = true,
                RefreshGrid = true,
                GridId = "UserGrid",
                RefreshGridUrl = "List"
            };

            var vm = UserViewMapper.ToAddEditViewModel(user, customerTypes, provinces, cities, form);
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

            var currentUser = await service.Get(vm.UserID);
            if (currentUser == null)
                return Json(new { success = false, message = "کاربر یافت نشد." });

            var model = UserViewMapper.ToAddEditModel(vm);
            model.ProfileImageUrl = string.IsNullOrWhiteSpace(currentUser.ProfileImageUrl)
                ? "/images/imageUsers/noimage.jpg"
                : currentUser.ProfileImageUrl;

            var uploaded = await TryUpload(vm.ProfileImage);
            if (uploaded is { Success: false })
                return Json(new OperationResult("UpdateUser").ToFailed(uploaded.Message));
            if (uploaded is { Success: true })
            {
                RemoveUserImage(currentUser.ProfileImageUrl);
                model.ProfileImageUrl = uploaded.FileUrl;
            }
            else if (vm.RemoveProfileImage)
            {
                RemoveUserImage(currentUser.ProfileImageUrl);
                model.ProfileImageUrl = "/images/imageUsers/noimage.jpg";
            }

            return Json(await service.UpdateUser(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(string userID)
        {
            var user = await service.Get(userID);
            var result = await service.DeleteUser(userID);
            if (result.Success)
                RemoveUserImage(user?.ProfileImageUrl);
            return Json(result);
        }

        [HttpGet]
        public async Task<JsonResult> GetCitiesByProvince(int provinceId)
        {
            var cities = await service.GetCitiesByProvinceId(provinceId);
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
            var user = await service.GetDetails(userID);
            if (user == null)
                return NotFound();

            return PartialView("_Details", user);
        }

        [HttpGet]
        public async Task<IActionResult> EligibleRewards(string userID, int rewardPage = 0, int historyPage = 0)
        {
            if (string.IsNullOrWhiteSpace(userID))
                return NotFound();

            await rewards.RefreshEligibility(userID);

            var user = await service.GetDetails(userID);
            if (user == null)
                return NotFound();

            var pageSize = PaginationViewModel.DefaultPageSize;
            var allItems = await rewards.GetEligibleCatalogsForUser(user.UserID);
            var allRequests = await rewards.GetUserRequests(user.UserID);
            var rewardSlice = CrudGridPager.Slice(allItems, rewardPage, pageSize);
            var history = CrudGridPager.Slice(allRequests, historyPage, pageSize);

            var vm = new EligibleRewardsDialogViewModel
            {
                UserID = user.UserID,
                CustomerName = $"{user.FirstName} {user.LastName}".Trim(),
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                TotalEarnedPoints = user.TotalEarnedPoints,
                TotalSettledPoints = user.TotalSettledPoints,
                RemainedPoints = await rewards.GetAvailablePoints(user.UserID),
                TotalRegisteredCards = user.TotalRegisteredCards,
                IsEligibleForReward = user.IsEligibleForReward,
                HasReceivedReward = user.HasReceivedReward,
                Items = rewardSlice.Items,
                RecentRequests = history.Items,
                RewardPage = rewardSlice.PageIndex,
                RewardPageCount = rewardSlice.PageCount,
                RewardRecordCount = rewardSlice.RecordCount,
                HistoryPage = history.PageIndex,
                HistoryPageCount = history.PageCount,
                HistoryRecordCount = history.RecordCount
            };

            return PartialView("_EligibleRewards", vm);
            //return view(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RequestReward(string userID, int rewardCatalogID)
        {
            if (string.IsNullOrWhiteSpace(userID))
                return Json(new { success = false, message = "کاربر نامعتبر است." });

            var result = await rewards.CreateRequest(userID, rewardCatalogID);
            return Json(new { success = result.Success, message = result.Message });
        }

        private async Task<FileUploadResult?> TryUpload(IFormFile? image)
        {
            if (image == null)
                return null;
            return await fileManager.UploadAsync(
                image, 5, new[] { "jpg", "jpeg", "png" },
                "images/imageUsers/uploads", "images/imageUsers/thumbnails");
        }

        private void RemoveUserImage(string? url)
        {
            if (string.IsNullOrWhiteSpace(url) || url == "/images/imageUsers/noimage.jpg")
                return;
            fileManager.Remove(url);
            fileManager.Remove(url.Replace("/images/imageUsers/uploads/", "/images/imageUsers/thumbnails/"));
        }
    }
}
