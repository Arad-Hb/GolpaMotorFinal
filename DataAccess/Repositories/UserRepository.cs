using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Product;
using DomainModel.ViewModels.User;
using Framework.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        private ApplicationUser ToDbModel(UserAddEditModel user)
        {
            return new ApplicationUser
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProvinceID = user.ProvinceID,
                CityID = user.CityID,
                Address = user.Address,
                PostalCode = user.PostalCode,
                IsActive = user.IsActive,
                CreditCartNumber = user.CreditCartNumber,
                IBAN = user.IBAN,
                AccountNumber = user.AccountNumber
            };
        }

        private UserAddEditModel ToViewModel(ApplicationUser user)
        {
            return new UserAddEditModel
            {
                UserID = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProvinceID = user.ProvinceID,
                CityID = user.CityID,
                Address = user.Address,
                PostalCode = user.PostalCode,
                IsActive = user.IsActive,
                CreditCartNumber = user.CreditCartNumber,
                IBAN = user.IBAN,
                AccountNumber = user.AccountNumber
            };
        }

        public async Task<OperationResult> Add(UserAddEditModel user)
        {
            var op = new OperationResult("Add User");

            try
            {
                var newUser = ToDbModel(user);

                newUser.EmailConfirmed = true;

                var result = await userManager.CreateAsync(newUser, "123456");

                if (!result.Succeeded)
                {
                    return op.ToFailed(string.Join(" | ",
                        result.Errors.Select(x => x.Description)));
                }

                await db.SaveChangesAsync();
                return op.ToSuccess("کاربر با موفقیت ثبت شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ثبت کاربر : " + ex.Message);
            }
        }

        public async Task<OperationResult> Update(UserAddEditModel user)
        {
            var op = new OperationResult("Update User");

            if (string.IsNullOrEmpty(user.UserID))
                return op.ToFailed("شناسه کاربر نامعتبر است");

            try
            {
                var dbUser = await userManager.FindByIdAsync(user.UserID);

                if (dbUser == null)
                    return op.ToFailed("کاربر یافت نشد");

                dbUser.FirstName = user.FirstName;
                dbUser.LastName = user.LastName;
                dbUser.Email = user.Email;
                dbUser.UserName = user.Email;
                dbUser.PhoneNumber = user.PhoneNumber;
                dbUser.ProvinceID = user.ProvinceID;
                dbUser.CityID = user.CityID;
                dbUser.Address = user.Address;
                dbUser.PostalCode = user.PostalCode;
                dbUser.ProfileImageUrl = user.ProfileImageUrl;
                dbUser.IsActive = user.IsActive;
                dbUser.CreditCartNumber = user.CreditCartNumber;
                dbUser.IBAN = user.IBAN;
                dbUser.AccountNumber = user.AccountNumber;

                dbUser.TotalSettledPoints = user.TotalSettledPoints;
                dbUser.TotalEarnedPoints = user.TotalEarnedPoints;
                dbUser.RemainedPoints = user.RemainedPoints;
                dbUser.TotalRegisteredCards = user.TotalRegisteredCards;

                

                var result = await userManager.UpdateAsync(dbUser);

                if (!result.Succeeded)
                {
                    return op.ToFailed(string.Join(" | ",
                        result.Errors.Select(x => x.Description)));
                }

                await db.SaveChangesAsync();
                return op.ToSuccess("اطلاعات کاربر با موفقیت ویرایش شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ویرایش کاربر : " + ex.Message);
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
                return null;

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
            if (!string.IsNullOrEmpty(sm.PhoneNumber))
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

