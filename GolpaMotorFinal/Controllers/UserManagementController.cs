using DataAccess.Repositories;
using DataAccess.Services;
using DomainModel.ViewModels.User;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Models.ViewModels.Account;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using GolpaMotorFinal.Models.ViewModels.CRUD;
using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly IUserRepository repo;
        private readonly IUserService service;
        private readonly IFileManager fileManager;
        private readonly IRewardRequestRepository rewardRequests;

        public UserManagementController(
            IUserRepository repo,
            IUserService service,
            IFileManager _fileManager,
            IRewardRequestRepository rewardRequests)
        {
            this.repo = repo;
            this.service = service;
            this.fileManager = _fileManager;
            this.rewardRequests = rewardRequests;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search([FromForm(Name = "Search.SearchTerm")] string? searchTerm)
        {
            return ViewComponent("UserList", new { searchTerm, pageIndex = 0 });
        }

        [HttpGet]
        public async Task<IActionResult> MergeAccounts(string userID, string? gridId = null, string? refreshUrl = null)
        {
            var currentUser =await service.GetUserMergeAccounts(userID);

            if (string.IsNullOrWhiteSpace(currentUser.UserID))
                return NotFound();

            ViewBag.GridId = string.IsNullOrWhiteSpace(gridId) ? "UserGrid" : gridId;
            ViewBag.RefreshUrl = string.IsNullOrWhiteSpace(refreshUrl) ? Url.Action("Grid", "UserManagement") : refreshUrl;

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

                return RedirectToAction(nameof(UserReport));
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
                return RedirectToAction(nameof(UserReport));
            }

            return RedirectToAction(nameof(UserReport));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchUserForMerge(MergeAccountsComplexViewModel model)
        {
            var found = await service.GetMergeSearchResult(model.Search?.SearchTerm ?? string.Empty);
            if (string.IsNullOrWhiteSpace(found.UserID) || found.UserID == model.CurrentUser?.UserID)
                model.SearchedUser = null;
            else
                model.SearchedUser = found;

            return PartialView("_MergeResult", model);
        }

        [HttpGet]
        public async Task<IActionResult> UserReportGrid(int pageIndex = 0)
        {
            var page = await service.GetUserReportPage(pageIndex);
            var grid = AttachUserReportPager(service.BuildUserReportGrid(page.Users), page.PageIndex, page.PageCount, page.RecordCount);
            return ViewComponent("CrudGrid", new { model = grid });
        }

        [HttpGet]
        public async Task<JsonResult> Get(string UserID)
        {
            var user = await repo.Get(UserID);
            return Json(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await repo.GetAll();
            return Json(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
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
                RefreshGridUrl = "Grid"
            };
            var vm = new UserAddEditViewModel
            {
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

            if (vm.ProfileImage != null)
            {
                var upload = await fileManager.UploadAsync(
                    vm.ProfileImage,
                    5,
                    new[] { "jpg", "jpeg", "png" },
                    "images/imageUsers/uploads",
                    "images/imageUsers/thumbnails");

                if (!upload.Success)
                {
                    return Json(new
                    {
                        success = false,
                        message = upload.Message
                    });
                }

                vm.ProfileImageUrl = upload.FileUrl;
            }

            var model = new UserAddEditModel
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                ProvinceID = vm.ProvinceID,
                CityID = vm.CityID,
                Address = vm.Address,
                PostalCode = vm.PostalCode,
                IsActive = vm.IsActive,
                ProfileImageUrl = vm.ProfileImageUrl
            };

            var op = await service.AddUser(model);

            if (!op.Success && vm.ProfileImage != null)
            {
                if (!string.IsNullOrWhiteSpace(vm.ProfileImageUrl))
                {
                    fileManager.Remove(vm.ProfileImageUrl);

                    var thumbnailPath = vm.ProfileImageUrl.Replace(
                        "/images/imageUsers/uploads/",
                        "/images/imageUsers/thumbnails/");

                    fileManager.Remove(thumbnailPath);
                }
            }

            return Json(op);
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

            var current = await repo.Get(vm.UserID);
            vm.ProfileImageUrl = string.IsNullOrWhiteSpace(current?.ProfileImageUrl)
                ? "/images/imageUsers/noimage.jpg"
                : current.ProfileImageUrl;

            if (vm.ProfileImage != null)
            {
                var upload = await fileManager.UploadAsync(
                    vm.ProfileImage,
                    5,
                    new[] { "jpg", "jpeg", "png" },
                    "images/imageUsers/uploads",
                    "images/imageUsers/thumbnails");

                if (!upload.Success)
                {
                    return Json(new
                    {
                        success = false,
                        message = upload.Message
                    });
                }

                if (upload.FileUrl == vm.ProfileImageUrl)
                {
                    fileManager.Remove(vm.ProfileImageUrl);
                    var thumbnailPath = vm.ProfileImageUrl.Replace(
                        "/images/imageUsers/uploads/",
                        "/images/imageUsers/thumbnails/");

                    fileManager.Remove(thumbnailPath);
                }

                vm.ProfileImageUrl = upload.FileUrl;
            }

            var model = new UserAddEditModel
            {
                UserID = vm.UserID,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                ProvinceID = vm.ProvinceID,
                CityID = vm.CityID,
                Address = vm.Address,
                PostalCode = vm.PostalCode,
                CreditCartNumber = vm.CreditCartNumber,
                IBAN = vm.IBAN,
                AccountNumber = vm.AccountNumber,
                IsActive = vm.IsActive,
                IsDeleted = vm.IsDeleted,
                ProfileImageUrl= vm.ProfileImageUrl
            };

            var result = await service.UpdateUser(model);

            return Json(result);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(string userID)
        {
            var result = await repo.Delete(userID);
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
            var rewards = SlicePage(allItems, rewardPage, pageSize);
            var history = SlicePage(allRequests, historyPage, pageSize);

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

        [HttpGet]
        public IActionResult Grid(string? searchTerm, int pageIndex = 0)
        {
            return ViewComponent("UserList", new { searchTerm, pageIndex });
        }


        [HttpGet]
        public async Task<IActionResult> UserReport(int pageIndex = 0)
        {
            var page = await service.GetUserReportPage(pageIndex);
            var grid = AttachUserReportPager(service.BuildUserReportGrid(page.Users), page.PageIndex, page.PageCount, page.RecordCount);
            return View(grid);
        }

        private static CrudGridViewModel AttachUserReportPager(CrudGridViewModel grid, int pageIndex, int pageCount, int recordCount)
        {
            grid.StartRowNumber = pageIndex * PaginationViewModel.DefaultPageSize + 1;
            grid.Pager = PaginationViewModel.For(
                "UserReportGrid",
                pageIndex,
                pageCount,
                recordCount,
                "/UserManagement/UserReportGrid");
            return grid;
        }

        private static (List<T> Items, int PageIndex, int PageCount, int RecordCount) SlicePage<T>(List<T> source, int pageIndex, int pageSize)
        {
            var count = source?.Count ?? 0;
            if (pageSize <= 0)
                pageSize = PaginationViewModel.DefaultPageSize;
            var pageCount = count == 0 ? 1 : (int)Math.Ceiling(count / (double)pageSize);
            if (pageIndex < 0)
                pageIndex = 0;
            if (pageIndex >= pageCount)
                pageIndex = pageCount - 1;
            var items = count == 0
                ? new List<T>()
                : source!.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return (items, pageIndex, pageCount, count);
        }

    }

}
