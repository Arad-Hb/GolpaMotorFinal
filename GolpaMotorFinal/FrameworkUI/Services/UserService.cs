using DataAccess.Services;
using DomainModel.ViewModels.User;
using Framework.Common;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using GolpaMotorFinal.Models.ViewModels.CRUD;
using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace GolpaMotorFinal.FrameworkUI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository repo;
        private readonly IFileManager fileManager;

        public UserService(IUserRepository repo, IFileManager fileManager)
        {
            this.repo = repo;
            this.fileManager = fileManager;
        }

        public async Task<OperationResult> DeleteUser(string userID)
        {
            var op = new OperationResult("DeleteUser");

            try
            {
                var user = await repo.Get(userID);

                if (user == null)
                    return op.ToFailed("کاربر یافت نشد");

                // تغییر: اول DB حذف انجام می‌شود (امن‌تر)
                var result = await repo.Delete(userID);

                if (!result.Success)
                    return result;

                // تغییر: بعد از موفقیت DB، فایل حذف می‌شود
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var path = fileManager.Remove(user.ProfileImageUrl);
                }

                return result;
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف کاربر: " + ex.Message);
            }
        }

        public async Task<OperationResult> AddUser(UserAddEditModel user)
        {
            var op = new OperationResult("AddUser");

            try
            {
                user.IsDeleted = false;

                return await repo.Add(user);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ثبت کاربر: " + ex.Message);
            }
        }

        public async Task<OperationResult> UpdateUser(UserAddEditModel user)
        {
            var op = new OperationResult("UpdateUser");

            try
            {
                var currentUser = await repo.Get(user.UserID);

                if (currentUser == null)
                    return op.ToFailed("کاربر یافت نشد.");

                return await repo.Update(user);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ویرایش کاربر: " + ex.Message);
            }
        }

        public async Task<UserAddEditViewModel?> GetForEdit(string userID)
        {
            var user = await repo.Get(userID);

            if (user == null) return null;

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
                RefreshGridUrl = "Grid"
            };

            var provinces = await repo.GetProvinces();
            var cities = user.ProvinceID.HasValue
                ? await repo.GetCitiesByProvinceId(user.ProvinceID.Value)
                : new List<DomainModel.Models.City>();

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
                CreditCartNumber = user.CreditCartNumber,
                IBAN = user.IBAN,
                AccountNumber = user.AccountNumber,
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted,
                ProfileImageUrl = user.ProfileImageUrl,
                Provinces = new SelectList(provinces, "ProvinceID", "Name", user.ProvinceID),
                Cities = new SelectList(cities, "CityID", "Name", user.CityID),
                CrudFormViewModel = form
            };

            return vm;
        }


        public async Task<List<UserListItemViewModel>> GetUsers()
        {
            var users = await repo.GetAll();

            var result = new List<UserListItemViewModel>();

            foreach (var u in users)
            {
                //var roleName = await repo.GetUserRoleById(u.UserID);

                result.Add(new UserListItemViewModel
                {
                    UserID = u.UserID,
                    ProfileImageUrl = u.ProfileImageUrl,
                    FullName = $"{u.FirstName ?? string.Empty} {u.LastName ?? string.Empty}".Trim(),
                    PhoneNumber = u.PhoneNumber ?? string.Empty,
                    //RoleName = roleName,
                    RoleName = u.RoleName ?? string.Empty,
                    TotalRegisteredCards = u.TotalRegisteredCards,
                    TotalEarnedPoints = u.TotalEarnedPoints,
                    IsEligibleForReward = u.IsEligibleForReward,
                    HasReceivedReward = u.HasReceivedReward,
                    Province = u.Province ?? string.Empty,
                    City = u.City ?? string.Empty
                });
            }

            return result;
        }

        public async Task<List<UserReportViewModel>> GetUserReport()
        {
            var users = await repo.GetAll();

            return users.Select(u => new UserReportViewModel
            {
                UserID = u.UserID,

                FullName = $"{u.FirstName ?? string.Empty} {u.LastName ?? string.Empty}".Trim(),

                PhoneNumber = u.PhoneNumber ?? string.Empty,
                RoleName = u.RoleName ?? string.Empty,

                TotalRegisteredCards = u.TotalRegisteredCards,
                TotalEarnedPoints = u.TotalEarnedPoints,
                TotalSettledPoints = u.TotalSettledPoints,
                RemainedPoints = u.RemainedPoints,
                ProfileImageUrl = u.ProfileImageUrl,

                Province = u.Province ?? string.Empty,
                City = u.City ?? string.Empty
            })
            .ToList();
        }

        public async Task<(List<UserReportViewModel> Users, int PageIndex, int PageCount, int RecordCount)> GetUserReportPage(UserSearchModel? sm = null)
        {
            sm ??= new UserSearchModel();
            sm.PageSize = PaginationViewModel.DefaultPageSize;
            var result = await repo.Search(sm);
            var page = result.sm ?? sm;
            var users = (result.userList ?? new List<UserListItem>()).Select(u => new UserReportViewModel
            {
                UserID = u.UserID,
                FullName = $"{u.FirstName ?? string.Empty} {u.LastName ?? string.Empty}".Trim(),
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                RoleName = u.RoleName ?? string.Empty,
                TotalRegisteredCards = u.TotalRegisteredCards,
                TotalEarnedPoints = u.TotalEarnedPoints,
                TotalSettledPoints = u.TotalSettledPoints,
                RemainedPoints = u.RemainedPoints,
                ProfileImageUrl = u.ExistingProfileImageUrl ?? string.Empty,
                Province = u.Province ?? string.Empty,
                City = u.City ?? string.Empty
            }).ToList();

            return (users, page.PageIndex, page.PageCount, page.RecordCount);
        }

        public async Task<MergeAccountsViewModel> GetUserMergeAccounts(string userID)
        {
            var user = await repo.GetDetails(userID);

            if (user == null)
                return new MergeAccountsViewModel();

            return new MergeAccountsViewModel
            {
                UserID = user.UserID,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                PhoneNumber = user.PhoneNumber,
                TotalEarnedPoints = user.TotalEarnedPoints,
                TotalSettledPoints = user.TotalSettledPoints,
                RemainedPoints = user.RemainedPoints,
                TotalRegisteredCards = user.TotalRegisteredCards,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }
        public async Task<MergeAccountsViewModel> GetMergeSearchResult(string sm)
        {
            var result = new MergeAccountsViewModel();

            var searchResult = await repo.GetUserDetail(sm);

            if (searchResult!=null)
            {
                result = new MergeAccountsViewModel
                {
                    UserID = searchResult.UserID,
                    FullName = searchResult.FirstName + " " + searchResult.LastName,
                    PhoneNumber = searchResult.PhoneNumber,
                    TotalEarnedPoints = searchResult.TotalEarnedPoints,
                    TotalSettledPoints = searchResult.TotalSettledPoints,
                    RemainedPoints = searchResult.RemainedPoints,
                    TotalRegisteredCards = searchResult.TotalRegisteredCards,
                    ProfileImageUrl = searchResult.ProfileImageUrl
                };
            }
            return result;

        }

        public async Task<OperationResult> MergeUsers(MergeAccountsViewModel model)
        {
            var op = new OperationResult("MergeUsersInUserManagement");

            if (model == null)
                return op.ToFailed("اطلاعات نامعتبر است.");

            if (string.IsNullOrWhiteSpace(model.SelectedMergeUserID))
                return op.ToFailed("لطفاً حساب مقصد را انتخاب کنید.");

            if (model.UserID == model.SelectedMergeUserID)
                return op.ToFailed("امکان ادغام یک کاربر با خودش وجود ندارد.");

            return await repo.MergeAccounts(model.UserID, model.SelectedMergeUserID);
        }

        public CrudGridViewModel BuildUserGrid(IEnumerable<UserListItemViewModel> users)
        {
            var grid = new CrudGridViewModel { GridId = "UserGrid" };
            grid.Headers.AddRange(new[] { "تصویر", "نام", "موبایل", "شغل", "کارت", "امتیاز", "واجد پاداش", "دریافت پاداش", "استان", "شهر" });

            foreach (var item in users)
            {
                var row = new GridRow { Key = item.UserID };
                row.Columns.Add(Image(item.ProfileImageUrl));
                row.Columns.Add(Text(item.FullName));
                row.Columns.Add(Text(item.PhoneNumber));
                row.Columns.Add(Text(item.RoleName));
                row.Columns.Add(Number(item.TotalRegisteredCards));
                row.Columns.Add(Number(item.TotalEarnedPoints));
                row.Columns.Add(Text(item.IsEligibleForReward ? "بله" : "خیر"));
                row.Columns.Add(Text(item.HasReceivedReward ? "بله" : "خیر"));
                row.Columns.Add(Text(item.Province));
                row.Columns.Add(Text(item.City));
                row.Actions.Add(new GridAction
                {
                    ActionText = "جزئیات",
                    OpenModal = true,
                    Icon = "fa fa-eye",
                    Url = "/UserManagement/Details",
                    Id = item.UserID,
                    CssClass = "btn btn-sm btn-outline-secondary"
                });
                grid.Rows.Add(row);
            }

            return grid;
        }

        public CrudGridViewModel BuildUserReportGrid(IEnumerable<UserReportViewModel> users)
        {
            var grid = new CrudGridViewModel { GridId = "UserReportGrid" };

            grid.Headers.AddRange(new[]
            {
                "تصویر", "نام", "موبایل", "شغل", "کارت", "امتیاز", "تسویه", "مانده", "استان", "شهر"
            });

            foreach (var item in users)
            {
                var row = new GridRow { Key = item.UserID };

                row.Columns.Add(Image(item.ProfileImageUrl));
                row.Columns.Add(Text(item.FullName));
                row.Columns.Add(Text(item.PhoneNumber));
                row.Columns.Add(Text(item.RoleName));
                row.Columns.Add(Number(item.TotalRegisteredCards));
                row.Columns.Add(Number(item.TotalEarnedPoints));
                row.Columns.Add(Number(item.TotalSettledPoints));
                row.Columns.Add(Number(item.RemainedPoints, "fw-bold text-success"));
                row.Columns.Add(Text(item.Province));
                row.Columns.Add(Text(item.City));

                row.Actions.Add(new GridAction
                {
                    ActionText = "جزئیات",
                    OpenModal = true,
                    EnableRefresh = true,
                    Ajax = true,
                    Icon = "fa fa-eye",
                    Url = "/UserManagement/Details",
                    Id = item.UserID,
                    CssClass = "btn btn-sm btn-outline-secondary"
                });

                row.Actions.Add(new GridAction
                {
                    ActionText = "ادغام",
                    OpenModal = true,
                    EnableRefresh = true,
                    Ajax = true,
                    Icon = "fa fa-user-plus",
                    Url = "/UserManagement/MergeAccounts?gridId=UserReportGrid&refreshUrl=/UserManagement/UserReportGrid",
                    Id = item.UserID,
                    CssClass = "btn btn-sm btn-outline-primary"
                });

                grid.Rows.Add(row);
            }

            return grid;
        }

        private GridColumn Number(object? value, string css = "")
        {
            return new GridColumn
            {
                Type = GridColumnType.Number,
                Value = value ?? 0,
                CssClass = css
            };
        }
        private GridColumn Text(object value,string css = "")
        {
            return new GridColumn
            {
                Type = GridColumnType.Text,
                Value = value ?? "-",
                CssClass = css
            };
        }
        private GridColumn Image(string url)
        {
            return new GridColumn
            {
                Type = GridColumnType.Image,
                ImageUrl = string.IsNullOrWhiteSpace(url)
                    ? "/images/avatar.png"
                    : url
            };
        }
        private GridColumn PersianDate(DateTime? value)
        {
            return new GridColumn
            {
                Type = GridColumnType.PersianDate,
                Value = value
            };
        }
        private GridColumn Boolean(bool value)
        {
            return new GridColumn
            {
                Type = GridColumnType.Boolean,
                Value = value,
                TrueIcon = "fa fa-check text-success",
                FalseIcon = "fa fa-times text-danger"
            };
        }
        private GridColumn Badge(string value,string css)
        {
            return new GridColumn
            {
                Type = GridColumnType.Badge,
                Value = value,
                BadgeClass = css
            };
        }
    }
}

