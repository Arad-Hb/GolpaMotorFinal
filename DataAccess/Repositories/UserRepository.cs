using DataAccess.Services;
using DomainModel.Models;
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

                var result = await userManager.UpdateAsync(dbUser);

                if (!result.Succeeded)
                {
                    return op.ToFailed(string.Join(" | ",
                        result.Errors.Select(x => x.Description)));
                }

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

                return op.ToSuccess("کاربر با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در حذف کاربر : " + ex.Message);
            }
        }

        public async Task<OperationResult> SoftDelete(string userID)
        {
            var op = new OperationResult("Delete User");

            try
            {
                var user = await userManager.FindByIdAsync(userID);

                if (user == null)
                    return op.ToFailed("کاربر یافت نشد");

                user.IsDeleted = true;

                var result = await userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return op.ToFailed("خطا در حذف کاربر");

                return op.ToSuccess("کاربر با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed(ex.Message);
            }
        }

        public async Task<bool> Exists(string userID)
        {
            return await db.Users
                .AnyAsync(x => x.Id == userID && !x.IsDeleted);
        }

        public async Task<UserAddEditModel?> Get(string userID)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userID && !x.IsDeleted);

            if (user == null)
                return null;

            return ToViewModel(user);
        }

        public async Task<List<UserListItem>> GetAll()
        {
            return await userManager.Users.Where(x => !x.IsDeleted)
                .Select(u => new UserListItem
                {
                    UserID = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    RegisterDate = u.RegisterDate,                   
                    Email = u.Email,
                    Province = u.Province != null ? u.Province.Name : "",
                    City = u.City != null ? u.City.Name : "",
                    PhoneNumber = u.PhoneNumber
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
                .Include(x => x.Province)
                .Include(x => x.City)
                .Where(x => x.Id == userID)
                .Select(x => new UserDetailsModel
                {
                    UserID = x.Id,

                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber!,

                    Province = x.Province.Name,
                    City = x.City.Name,
                    Address = x.Address,
                    PostalCode = x.PostalCode,

                    ProfileImageUrl = x.ProfileImageUrl,

                    IsActive = x.IsActive,
                    RegisterDate = x.RegisterDate,

                    CreditCartNumber = x.CreditCartNumber,
                    IBAN = x.IBAN,
                    AccountNumber = x.AccountNumber,

                    TotalEarnedPoints = x.TotalEarnedPoints,
                    TotalSettledPoints = x.TotalSettledPoints,
                    RemainedPoints = x.RemainedPoints,
                    TotalRegisteredCards = x.TotalRegisteredCards
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<Province>> GetProvinces()
        {
            return await db.Provinces.ToListAsync();
        }

        public async Task<List<City>> GetCitiesByProvinceId(int provinceId)
        {
            return await db.Cities.Where(c => c.ProvinceID == provinceId).ToListAsync();
        }

        public Task<UserListComplexModel> Search(UserSearchModel sm)
        {
            throw new NotImplementedException();
        }
    }
}
