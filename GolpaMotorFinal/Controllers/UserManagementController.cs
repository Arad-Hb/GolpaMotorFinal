using DataAccess.Repositories;
using DataAccess.Services;
using DomainModel.ViewModels.User;
using GolpaMotorFinal.FrameworkUI.Services;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using GolpaMotorFinal.Models.ViewModels.CRUD;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromForm(Name = "Search.SearchTerm")] string? searchTerm)
        {
            var sm = new UserSearchModel { PhoneNumber = searchTerm, SearchTerm = searchTerm };
            var result = await repo.Search(sm);

            var users = result.userList.Select(u => new UserListItemViewModel
            {
                UserID = u.UserID,
                FullName = $"{u.FirstName ?? string.Empty} {u.LastName ?? string.Empty}".Trim(),
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                ProfileImageUrl = u.ExistingProfileImageUrl ?? string.Empty,
                Province = u.Province ?? string.Empty,
                City = u.City ?? string.Empty
            }).ToList();

            return ViewComponent("UserList", users);
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
        public async Task<IActionResult> UserReportGrid()
        {
            var users = await service.GetUserReport();
            var grid = service.BuildUserReportGrid(users);
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

            var users = await service.GetUserReport();

            var grid = service.BuildUserReportGrid(users);

            return View(grid);
            //        var users = await service.GetUserReport();

            //        var grid = new CrudGridViewModel
            //        {
            //            Items = users.Cast<object>(),

            //            Columns =
            //{
            //    new GridColumn
            //    {
            //        Header = "نام",
            //        PropertyName = nameof(UserReportViewModel.FullName)
            //    },

            //    new GridColumn
            //    {
            //        Header = "موبایل",
            //        PropertyName = nameof(UserReportViewModel.PhoneNumber)
            //    },

            //    new GridColumn
            //    {
            //        Header = "استان",
            //        PropertyName = nameof(UserReportViewModel.Province)
            //    },

            //    new GridColumn
            //    {
            //        Header = "شهر",
            //        PropertyName = nameof(UserReportViewModel.City)
            //    },

            //    new GridColumn
            //    {
            //        Header = "تصویر",
            //        PropertyName = nameof(UserReportViewModel.ProfileImageUrl),
            //        Type = GridColumnType.Image
            //    }
            //},

            //            Actions =
            //{
            //    new GridAction
            //    {
            //        Title = "جزئیات",
            //        Icon = "fa fa-eye",
            //        CssClass = "btn btn-sm btn-secondary",
            //        Url = Url.Action("Details","UserManagement"),
            //        IdProperty = nameof(UserReportViewModel.UserID)
            //    },

            //    new GridAction
            //    {
            //        Title = "ادغام",
            //        Icon = "fa fa-user-plus",
            //        CssClass = "btn btn-sm btn-warning",
            //        Url = Url.Action("MergeAccounts","UserManagement"),
            //        IdProperty = nameof(UserReportViewModel.UserID)
            //    }
            //}
            //        };



            //        grid.Columns.Add(new()
            //        {
            //            Header = "کارت ثبت شده",
            //            Value = x => x.TotalRegisteredCards
            //        });

            //        grid.Columns.Add(new()
            //        {
            //            Header = "امتیاز کسب شده",
            //            Value = x => x.TotalEarnedPoints
            //        });

            //        grid.Columns.Add(new()
            //        {
            //            Header = "امتیاز تسویه شده",
            //            Value = x => x.TotalSettledPoints
            //        });

            //        grid.Columns.Add(new()
            //        {
            //            Header = "مانده امتیاز",
            //            Value = x => x.RemainedPoints,
            //            CssClass = "fw-bold text-success"
            //        });

            //        grid.Actions.Add(new()
            //        {
            //            Title = "جزئیات",
            //            Icon = "fa fa-eye",
            //            CssClass = "btn btn-sm btn-secondary open-modal",
            //            Url = Url.Action("Details", "UserManagement"),
            //            Id = x => x.UserID
            //        });

            //        grid.Actions.Add(new()
            //        {
            //            Title = "ادغام",
            //            Icon = "fa fa-user-plus",
            //            CssClass = "btn btn-sm btn-warning open-modal",
            //            Url = Url.Action("MergeAccounts", "UserManagement"),
            //            Id = x => x.UserID
            //        });

            //        return View(grid);
        }


        //[HttpGet]
        //public async Task<IActionResult> UserReport()
        //{
        //    var model = await service.GetUserReport();
        //    return View(model);
        //}

    }

}
