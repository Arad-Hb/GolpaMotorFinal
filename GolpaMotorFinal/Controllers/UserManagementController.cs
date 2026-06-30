using DataAccess.Repositories;
using DataAccess.Services;
using DomainModel.ViewModels.User;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Models.ViewModels;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GolpaMotorFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly IUserRepository repo;
        private readonly IUserService service;
        public UserManagementController(IUserRepository repo, IUserService service)
        {
            this.repo = repo;
            this.service = service;
        }

        public async Task<IActionResult> Index()
        {
            var model = await service.GetUsers();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MergeAccounts(string userID)
        {
            var currentUser =await service.GetUserMergeAccounts(userID);

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
            var result=await service.MergeUsers(model.CurrentUser);

            return RedirectToAction(nameof(UserReport));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchUserForMerge(MergeAccountsComplexViewModel model)
        {
            model.SearchedUser =await service.GetMergeSearchResult(model.Search.SearchTerm); 

            return PartialView("_MergeResult", model);
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
            return PartialView("_Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(UserAddEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "اطلاعات معتبر نیست" });
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
                IsActive = vm.IsActive
            };

            var result = await service.AddUser(model, vm.ProfileImage);

            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string userID)
        {
            var user = await repo.Get(userID);

            if (user == null)
                return NotFound();

            var vm = new UserAddEditViewModel
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProvinceID = user.ProvinceID,
                CityID = user.CityID,
                Address = user.Address,
                PostalCode = user.PostalCode,
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted
            };

            return PartialView("_Edit", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Edit(UserAddEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات معتبر نیست" });

            // تبدیل ViewModel → Domain Model
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
                IsActive = vm.IsActive,
                IsDeleted = vm.IsDeleted
            };

            // ارسال به Service همراه فایل جدید
            var result = await service.UpdateUser(model, vm.ProfileImage);

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
        public async Task<IActionResult> Details(string userID)
        {
            var user = await repo.GetDetails(userID);

            if (user == null)
                return NotFound();

            return PartialView("_Details", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RemovePicture(string userID)
        {
            var result = await service.RemovePicture(userID);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> UserList(List<UserListItemViewModel> model)
        {
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UserReport()
        {
            var model = await service.GetUserReport();
            return View(model);
        }
      
       

    }

}
