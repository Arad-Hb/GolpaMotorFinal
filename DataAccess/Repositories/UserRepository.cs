using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.User;
using Framework.Common;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


namespace DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly GolpaMotorDbContext db;
        private readonly UserManager<ApplicationUser> userManager;        

        public UserRepository(GolpaMotorDbContext db,UserManager<ApplicationUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;            
        }

        private ApplicationUser ToDbModel(UserAddEditModel model)
        {
            var user = new ApplicationUser
            {
                UserName = string.IsNullOrWhiteSpace(model.Email)
                            ? $"noemail_{Guid.NewGuid():N}@noemail.local"
                            : model.Email.Trim(),

                Email = string.IsNullOrWhiteSpace(model.Email)
                            ? $"noemail_{Guid.NewGuid():N}@noemail.local"
                            : model.Email.Trim(),

                PhoneNumber = model.PhoneNumber,

                FirstName = model.FirstName?.Trim(),
                LastName = model.LastName?.Trim(),

                ProvinceID = model.ProvinceID,
                CityID = model.CityID,

                Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim(),
                PostalCode = string.IsNullOrWhiteSpace(model.PostalCode) ? null : model.PostalCode.Trim(),

                ProfileImageUrl = string.IsNullOrWhiteSpace(model.ProfileImageUrl)
                    ? null
                    : model.ProfileImageUrl,

                CreditCartNumber = string.IsNullOrWhiteSpace(model.CreditCartNumber)
                    ? null
                    : model.CreditCartNumber,

                IBAN = string.IsNullOrWhiteSpace(model.IBAN)
                    ? null
                    : model.IBAN,

                AccountNumber = string.IsNullOrWhiteSpace(model.AccountNumber)
                    ? null
                    : model.AccountNumber,

                IsActive = model.IsActive,
                IsDeleted = false,
                IsConfirmedCode = false,

                RegisterDate = model.RegisterDate ?? DateTime.Now,

                TotalEarnedPoints = model.TotalEarnedPoints ?? 0,
                TotalSettledPoints = model.TotalSettledPoints ?? 0,
                TotalRegisteredCards = model.TotalRegisteredCards ?? 0
            };

            user.RemainedPoints =
                (user.TotalEarnedPoints ?? 0) -
                (user.TotalSettledPoints ?? 0);

            return user;
        }

        private void UpdateDbModel(ApplicationUser user, UserAddEditModel model)
        {
            user.FirstName = model.FirstName?.Trim();
            user.LastName = model.LastName?.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();

            user.ProvinceID = model.ProvinceID;
            user.CityID = model.CityID;

            user.Address = model.Address?.Trim();
            user.PostalCode = model.PostalCode?.Trim();

            user.ProfileImageUrl = model.ProfileImageUrl?.Trim();

            user.CreditCartNumber = model.CreditCartNumber?.Trim();
            user.IBAN = model.IBAN?.Trim();
            user.AccountNumber = model.AccountNumber?.Trim();

            user.IsActive = model.IsActive;

            user.TotalEarnedPoints = model.TotalEarnedPoints ?? 0;
            user.TotalSettledPoints = model.TotalSettledPoints ?? 0;
            user.TotalRegisteredCards = model.TotalRegisteredCards ?? 0;

            user.RemainedPoints =
                (user.TotalEarnedPoints ?? 0) -
                (user.TotalSettledPoints ?? 0);
        }

        private UserAddEditModel ToViewModel(ApplicationUser model)
        {
            var email = model.Email;
            var emailAttr = new EmailAddressAttribute();
            if (!string.IsNullOrWhiteSpace(email) && !emailAttr.IsValid(email))
            {
                email = null;
            }

            return new UserAddEditModel
            {
                UserID = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = email,
                PhoneNumber = model.PhoneNumber,

                ProvinceID = model.ProvinceID,
                CityID = model.CityID,
                Address = model.Address,
                PostalCode = model.PostalCode,
                ProfileImageUrl = model.ProfileImageUrl,

                IsActive = model.IsActive,
                IsDeleted = model.IsDeleted,

                CreditCartNumber = model.CreditCartNumber,
                IBAN = model.IBAN,
                AccountNumber = model.AccountNumber,

                TotalEarnedPoints = model.TotalEarnedPoints ?? 0,
                TotalSettledPoints = model.TotalSettledPoints ?? 0,
                RemainedPoints = model.RemainedPoints ?? 0,
                TotalRegisteredCards = model.TotalRegisteredCards ?? 0
            };
        }

        public async Task<OperationResult> Add(UserAddEditModel model)
        {
            var result = new OperationResult("Add User");

            try
            {
                var user = ToDbModel(model);
                var password = $"P@ss{Guid.NewGuid():N}1!";
                var identityResult = await userManager.CreateAsync(user, password);

                if (!identityResult.Succeeded)
                {
                    return result.ToFailed(string.Join(Environment.NewLine,
                        identityResult.Errors.Select(x => x.Description)));
                }

                return result.ToSuccess("کاربر با موفقیت ثبت شد");
            }
            catch (Exception ex)
            {
                return result.ToFailed("خطا در ثبت کاربر : " + ex.Message);
            }
        }

        public async Task<OperationResult> Update(UserAddEditModel model)
        {
            var op = new OperationResult("Update User");

            try
            {
                if (string.IsNullOrWhiteSpace(model.UserID))
                    return op.ToFailed("شناسه کاربر نامعتبر است");

                var user = await userManager.FindByIdAsync(model.UserID);

                if (user == null)
                    return op.ToFailed("کاربر یافت نشد");

                UpdateDbModel(user, model);

                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    user.Email = model.Email.Trim();
                }

                var result = await userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return op.ToFailed(string.Join(" | ", result.Errors.Select(x => x.Description)));

                return op.ToSuccess("اطلاعات کاربر با موفقیت ویرایش شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed($"خطا در ویرایش کاربر: {ex.Message}");
            }
        }

        public async Task<OperationResult> Delete(string userID)
        {
            var op = new OperationResult("Delete User");

            try
            {
                var user = await userManager.FindByIdAsync(userID);

                if (user == null)
                    return op.ToFailed("کاربر یافت نشد");

                var result = await userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    return op.ToFailed(string.Join(" | ",
                        result.Errors.Select(x => x.Description)));
                }

                await db.SaveChangesAsync();
                return op.ToSuccess("کاربر با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف کاربر : " + ex.Message);
            }
        }

        public async Task<OperationResult> SoftDelete(string userID)
        {
            var op = new OperationResult("Soft Delete User");

            try
            {
                var user = await userManager.FindByIdAsync(userID);

                if (user == null)
                    return op.ToFailed("کاربر یافت نشد");

                user.IsDeleted = true;

                var result = await userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return op.ToFailed("خطا در حذف کاربر");

                await db.SaveChangesAsync();
                return op.ToSuccess("کاربر با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed(ex.Message);
            }
        }

        public async Task<OperationResult> MergeAccounts(string currentUserID, string mergeUserID)
        {
            var op = new OperationResult("MergeAccounts");

            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var currentUser = await Get(currentUserID);

                if (currentUser == null)
                    return op.ToFailed("کاربر اصلی یافت نشد.");

                var mergeUser = await Get(mergeUserID);

                if (mergeUser == null)
                    return op.ToFailed("کاربر انتخاب شده یافت نشد.");

                // Merge statistics
                currentUser.TotalSettledPoints += mergeUser.TotalSettledPoints;
                currentUser.TotalEarnedPoints += mergeUser.TotalEarnedPoints;
                currentUser.TotalRegisteredCards += mergeUser.TotalRegisteredCards;
                currentUser.RemainedPoints += mergeUser.RemainedPoints;

                currentUser.IsActive = true;

                // Disable merged account
                mergeUser.IsActive = false;

                var deleteResult = await SoftDelete(mergeUser.UserID);

                if (!deleteResult.Success)
                {
                    await transaction.RollbackAsync();
                    return op.ToFailed(deleteResult.Message);
                }

                var updateCurrentUser = await Update(currentUser);

                if (!updateCurrentUser.Success)
                {
                    await transaction.RollbackAsync();
                    return op.ToFailed("تغییرات در حساب کاربری مورد نظر در هنگام ادغام با خطا متوقف شد.");
                }

                var updateMergeUser = await Update(mergeUser);

                if (!updateMergeUser.Success)
                {
                    await transaction.RollbackAsync();
                    return op.ToFailed("به‌روزرسانی حساب کاربری ادغام‌شونده با خطا مواجه شد.");
                }

                await transaction.CommitAsync();

                return op.ToSuccess("حساب‌های کاربری با موفقیت ادغام شدند.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return op.ToFailed("در هنگام ادغام حساب‌ها خطایی رخ داد.");
            }
        }

        public async Task<bool> Exists(string userID)
        {
            return await db.Users
                .AnyAsync(x => x.Id == userID && !x.IsDeleted);
        }

        public async Task<UserAddEditModel> Get(string userID)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userID && !x.IsDeleted);

            if (user == null)
                return new UserAddEditModel();

            return ToViewModel(user);
        }

        public async Task<UserDetailsModel> GetUserDetail(string sm)
        {
            var result = new UserDetailsModel();
          
            var searchResult=await db.Users
               .Where(x => x.PhoneNumber == sm || x.FirstName==sm || x.LastName==sm)
               .Select(x => new UserDetailsModel
               {
                   UserID = x.Id,

                   FirstName = x.FirstName ?? string.Empty,
                   LastName = x.LastName ?? string.Empty,
                   Email = x.Email ?? string.Empty,
                   PhoneNumber = x.PhoneNumber ?? string.Empty,

                   Province = x.Province != null ? x.Province.Name : string.Empty,
                   City = x.City != null ? x.City.Name : string.Empty,
                   Address = x.Address ?? string.Empty,
                   PostalCode = x.PostalCode ?? string.Empty,

                   ProfileImageUrl = x.ProfileImageUrl ?? string.Empty,

                   RoleName = x.UserCustomerTypes
                       .Select(uct => uct.CustomerType.Title)
                       .FirstOrDefault() ?? string.Empty,

                   IsActive = x.IsActive,
                   RegisterDate = x.RegisterDate,

                   CreditCartNumber = x.CreditCartNumber ?? string.Empty,
                   IBAN = x.IBAN ?? string.Empty,
                   AccountNumber = x.AccountNumber ?? string.Empty,

                   TotalEarnedPoints = x.TotalEarnedPoints ?? 0,
                   TotalSettledPoints = x.TotalSettledPoints ?? 0,
                   RemainedPoints = x.RemainedPoints ?? 0,
                   TotalRegisteredCards = x.TotalRegisteredCards ?? 0
               })
               .FirstOrDefaultAsync();

            if (searchResult != null) result = searchResult;
            return result;
        }

        public async Task<List<UserDetailsModel>> GetAll()
        {
            return await db.Users
                .Where(x => !x.IsDeleted)
                .Select(x => new UserDetailsModel
                {
                    UserID = x.Id,

                    FirstName = x.FirstName ?? string.Empty,
                    LastName = x.LastName ?? string.Empty,
                    Email = x.Email ?? string.Empty,
                    PhoneNumber = x.PhoneNumber ?? string.Empty,
                    ProfileImageUrl = x.ProfileImageUrl ?? string.Empty,

                    Province = x.Province != null ? x.Province.Name : string.Empty,
                    City = x.City != null ? x.City.Name : string.Empty,
                    Address = x.Address ?? string.Empty,
                    PostalCode = x.PostalCode ?? string.Empty,

                    RoleName =x.UserCustomerTypes
                        .Select(c => c.CustomerType.Title)
                        .FirstOrDefault() ?? string.Empty,

                    IsActive = x.IsActive,
                    RegisterDate = x.RegisterDate,

                    CreditCartNumber = x.CreditCartNumber ?? string.Empty,
                    IBAN = x.IBAN ?? string.Empty,
                    AccountNumber = x.AccountNumber ?? string.Empty,

                    TotalEarnedPoints = x.TotalEarnedPoints ?? 0,
                    TotalSettledPoints = x.TotalSettledPoints ?? 0,
                    RemainedPoints = x.RemainedPoints ?? 0,
                    TotalRegisteredCards = x.TotalRegisteredCards ?? 0
                })
                .ToListAsync();
        }

        public async Task RemoveImage(string userID)
        {
            var user = await db.Users
                .FirstOrDefaultAsync(x => x.Id == userID);

            if (user != null)
            {
                user.ProfileImageUrl = null;
                await db.SaveChangesAsync();
            }
        }

        public async Task<UserDetailsModel?> GetDetails(string userID)
        {
            return await db.Users
                .Where(x => x.Id == userID)
                .Select(x => new UserDetailsModel
                {
                    UserID = x.Id,

                    FirstName = x.FirstName ?? string.Empty,
                    LastName = x.LastName ?? string.Empty,
                    Email = x.Email ?? string.Empty,
                    PhoneNumber = x.PhoneNumber ?? string.Empty,

                    Province = x.Province != null ? x.Province.Name : string.Empty,
                    City = x.City != null ? x.City.Name : string.Empty,
                    Address = x.Address ?? string.Empty,
                    PostalCode = x.PostalCode ?? string.Empty,

                    ProfileImageUrl = x.ProfileImageUrl ?? string.Empty,

                    RoleName = x.UserCustomerTypes
                        .Select(uct => uct.CustomerType.Title)
                        .FirstOrDefault() ?? string.Empty,

                    IsActive = x.IsActive,
                    RegisterDate = x.RegisterDate,

                    CreditCartNumber = x.CreditCartNumber ?? string.Empty,
                    IBAN = x.IBAN ?? string.Empty,
                    AccountNumber = x.AccountNumber ?? string.Empty,

                    TotalEarnedPoints = x.TotalEarnedPoints ?? 0,
                    TotalSettledPoints = x.TotalSettledPoints ?? 0,
                    RemainedPoints = x.RemainedPoints ?? 0,
                    TotalRegisteredCards = x.TotalRegisteredCards ?? 0
                })
                .FirstOrDefaultAsync();
        }

        public async Task<string> GetUserRoleById(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
                return string.Empty;

            var roles = await userManager.GetRolesAsync(user);

            return roles.FirstOrDefault() ?? string.Empty;
        }

        public async Task<List<Province>> GetProvinces()
        {
            return await db.Provinces.ToListAsync();
        }

        public async Task<List<City>> GetCitiesByProvinceId(int provinceId)
        {
            return await db.Cities.Where(c => c.ProvinceID == provinceId).ToListAsync();
        }

        public async Task<UserListComplexModel> Search(UserSearchModel sm)
        {

            var q = db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(sm.FirstName))
            {
                q = q.Where(u => u.FirstName.Contains(sm.FirstName));
            }
            if (!string.IsNullOrEmpty(sm.LastName))
            {
                q = q.Where(u => u.LastName.Contains(sm.LastName));
            }
            if (!string.IsNullOrEmpty(sm.SearchTerm))
            {
                q = q.Where(u =>
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(sm.SearchTerm)) ||
                    (u.FirstName != null && u.FirstName.Contains(sm.SearchTerm)) ||
                    (u.LastName != null && u.LastName.Contains(sm.SearchTerm)));
            }
            if (!string.IsNullOrEmpty(sm.PhoneNumber) && string.IsNullOrEmpty(sm.SearchTerm))
            {
                q = q.Where(u => u.PhoneNumber.Contains(sm.PhoneNumber));
            }
            if (!string.IsNullOrEmpty(sm.Email))
            {
                q = q.Where(u => u.Email.Contains(sm.Email));
            }
            var users = await q.Select(u => new UserListItem
            {
                UserID = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                ExistingProfileImageUrl=u.ProfileImageUrl,
                RemainedPoints= u.RemainedPoints?? 0,
            }).ToListAsync();

            var result=new UserListComplexModel { userList = users };

            return result;
        
         }

    }
}

