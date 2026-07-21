using DataAccess.Repositories;
using DataAccess.Services;
using DomainModel.ViewModels.User;
using Framework.Common;
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
        private readonly IFileManager fileManager;
        public UserManagementController(IUserRepository repo, IUserService service, IFileManager _fileManager)
        {
            this.repo = repo;
            this.service = service;
            this.fileManager = _fileManager;
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
                RefreshGridUrl = "/UserManagement/Grid"
                //RefreshGridUrl = Url.Action("Grid", "UserManagement") 
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
            var user = await repo.Get(userID);

            if (user == null) return NotFound();

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
                RefreshGridUrl = "/UserManagement/Grid"
            };

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
                IsDeleted = user.IsDeleted,
                ProfileImageUrl=user.ProfileImageUrl,  //set NoImage by ImageHelper in view
                CrudFormViewModel = form
            };

            return PartialView("_Edit", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Edit(UserAddEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "اطلاعات معتبر نیست" });

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
                IsActive = vm.IsActive,
                IsDeleted = vm.IsDeleted,
                ProfileImageUrl= vm.ProfileImageUrl
            };

            var result = await service.UpdateUser(model);

            return Json(result);
        }


        [HttpPost]
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

        [HttpGet]
        public IActionResult Grid()
        {
            return ViewComponent("UserList");
        }

        [HttpGet]
        public async Task<IActionResult> UserReport()
        {
            var model = await service.GetUserReport();
            return View(model);
        }

    }

}
