using DataAccess.Helpers;
using DataAccess.Mappers;
using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.User;
using Framework.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;


namespace DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly GolpaMotorDbContext db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserRepository(
            GolpaMotorDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<OperationResult> Add(UserAddEditModel model)
        {
            var result = new OperationResult("Add User");

            try
            {
                var user = UserMapper.ToEntity(model);
                var password = $"P@ss{Guid.NewGuid():N}1!";
                var identityResult = await userManager.CreateAsync(user, password);

                if (!identityResult.Succeeded)
                {
                    return result.ToFailed(string.Join(Environment.NewLine,
                        identityResult.Errors.Select(x => x.Description)));
                }

                await SyncUserCustomerType(user.Id, model.CustomerTypeID);

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

                UserMapper.Apply(user, model);

                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    user.Email = model.Email.Trim();
                }

                var result = await userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return op.ToFailed(string.Join(" | ", result.Errors.Select(x => x.Description)));

                await SyncUserCustomerType(model.UserID, model.CustomerTypeID);

                return op.ToSuccess("اطلاعات کاربر با موفقیت ویرایش شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed($"خطا در ویرایش کاربر: {ex.Message}");
            }
        }

        private async Task SyncUserCustomerType(string userID, int? customerTypeID)
        {
            var existing = await db.UserCustomerTypes
                .Where(x => x.UserID == userID)
                .ToListAsync();

            var selectedId = customerTypeID.GetValueOrDefault();
            if (selectedId > 0
                && existing.Count == 1
                && existing[0].CustomerTypeID == selectedId)
            {
                return;
            }

            if (existing.Count > 0)
                db.UserCustomerTypes.RemoveRange(existing);

            if (selectedId > 0)
            {
                await db.UserCustomerTypes.AddAsync(new UserCustomerType
                {
                    UserID = userID,
                    CustomerTypeID = selectedId
                });
            }

            await db.SaveChangesAsync();
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

            if (string.IsNullOrWhiteSpace(currentUserID) || string.IsNullOrWhiteSpace(mergeUserID))
                return op.ToFailed("اطلاعات ادغام نامعتبر است.");

            if (currentUserID == mergeUserID)
                return op.ToFailed("امکان ادغام یک کاربر با خودش وجود ندارد.");

            var currentUser = await db.Users.FirstOrDefaultAsync(x => x.Id == currentUserID && !x.IsDeleted);
            var mergeUser = await db.Users.FirstOrDefaultAsync(x => x.Id == mergeUserID && !x.IsDeleted);

            if (currentUser == null)
                return op.ToFailed("کاربر اصلی یافت نشد.");

            if (mergeUser == null)
                return op.ToFailed("کاربر انتخاب شده یافت نشد.");

            if (await userManager.IsInRoleAsync(mergeUser, "Admin"))
                return op.ToFailed("امکان ادغام حساب مدیر وجود ندارد.");

            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var sourceCards = await db.CardRegistrations
                    .Where(x => x.UserID == mergeUserID)
                    .ToListAsync();
                foreach (var card in sourceCards)
                    card.UserID = currentUserID;

                var sourceTx = await db.PointTransactions
                    .Where(x => x.UserID == mergeUserID)
                    .ToListAsync();
                foreach (var tx in sourceTx)
                    tx.UserID = currentUserID;

                var sourceRewards = await db.RewardRequests
                    .Where(x => x.UserID == mergeUserID)
                    .ToListAsync();
                foreach (var reward in sourceRewards)
                    reward.UserID = currentUserID;

                var sourceTypes = await db.UserCustomerTypes
                    .Where(x => x.UserID == mergeUserID)
                    .ToListAsync();
                var currentTypeIds = await db.UserCustomerTypes
                    .Where(x => x.UserID == currentUserID)
                    .Select(x => x.CustomerTypeID)
                    .ToListAsync();

                var typesToAdd = sourceTypes
                    .Select(x => x.CustomerTypeID)
                    .Where(id => !currentTypeIds.Contains(id))
                    .Distinct()
                    .ToList();

                if (sourceTypes.Count > 0)
                    db.UserCustomerTypes.RemoveRange(sourceTypes);

                foreach (var typeId in typesToAdd)
                {
                    await db.UserCustomerTypes.AddAsync(new UserCustomerType
                    {
                        UserID = currentUserID,
                        CustomerTypeID = typeId
                    });
                }

                await db.SaveChangesAsync();

                var earned = await db.PointTransactions
                    .Where(x => x.UserID == currentUserID && x.PointsAmount > 0)
                    .SumAsync(x => (int?)x.PointsAmount) ?? 0;
                var settled = Math.Abs(await db.PointTransactions
                    .Where(x => x.UserID == currentUserID && x.PointsAmount < 0)
                    .SumAsync(x => (int?)x.PointsAmount) ?? 0);

                currentUser.TotalEarnedPoints = earned;
                currentUser.TotalSettledPoints = settled;
                currentUser.RemainedPoints = earned - settled;
                currentUser.TotalRegisteredCards = await db.CardRegistrations.CountAsync(x => x.UserID == currentUserID);
                currentUser.IsActive = true;
                if (mergeUser.HasReceivedReward)
                    currentUser.HasReceivedReward = true;

                await RewardEligibilityHelper.ApplyToUserAsync(db, currentUser);

                mergeUser.IsActive = false;
                mergeUser.IsDeleted = true;
                mergeUser.TotalEarnedPoints = 0;
                mergeUser.TotalSettledPoints = 0;
                mergeUser.RemainedPoints = 0;
                mergeUser.TotalRegisteredCards = 0;
                mergeUser.IsEligibleForReward = false;
                mergeUser.PhoneNumber = $"merged_{mergeUser.Id}";
                mergeUser.UserName = $"merged_{mergeUser.Id}";
                mergeUser.NormalizedUserName = mergeUser.UserName.ToUpperInvariant();
                if (!string.IsNullOrWhiteSpace(mergeUser.Email))
                {
                    mergeUser.Email = $"merged_{mergeUser.Id}@merged.local";
                    mergeUser.NormalizedEmail = mergeUser.Email.ToUpperInvariant();
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return op.ToSuccess("حساب‌های کاربری با موفقیت ادغام شدند.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return op.ToFailed("در هنگام ادغام حساب‌ها خطایی رخ داد: " + ex.Message);
            }
        }

        public async Task<bool> Exists(string userID)
        {
            return await db.Users
                .AnyAsync(x => x.Id == userID && !x.IsDeleted);
        }

        public async Task<UserAddEditModel?> Get(string userID)
        {
            var user = await db.Users
                .Include(x => x.UserCustomerTypes)
                .FirstOrDefaultAsync(x => x.Id == userID && !x.IsDeleted);

            if (user == null)
                return null;

            return UserMapper.ToAddEditModel(user);
        }

        public async Task<UserDetailsModel> GetUserDetail(string sm)
        {
            var result = new UserDetailsModel();

            var searchResult = await db.Users
               .Where(x => !x.IsDeleted && (x.PhoneNumber == sm || x.FirstName == sm || x.LastName == sm))
               .Select(UserMapper.ToDetails)
               .FirstOrDefaultAsync();

            if (searchResult != null) result = searchResult;
            return result;
        }

        public async Task<List<UserDetailsModel>> GetAll()
        {
            return await db.Users
                .Where(x => !x.IsDeleted)
                .Select(UserMapper.ToDetails)
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
                .Select(UserMapper.ToDetails)
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

        public async Task<UserListComplexModel> Search(UserSearchModel sm)
        {
            var q = db.Users.Where(u => !u.IsDeleted).AsQueryable();

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
            if (sm.CustomerTypeID.HasValue && sm.CustomerTypeID.Value > 0)
            {
                q = q.Where(u => u.UserCustomerTypes.Any(t => t.CustomerTypeID == sm.CustomerTypeID.Value));
            }
            if (sm.ProvinceID.HasValue && sm.ProvinceID.Value > 0)
            {
                q = q.Where(u => u.ProvinceID == sm.ProvinceID.Value);
            }
            if (sm.CityID.HasValue && sm.CityID.Value > 0)
            {
                q = q.Where(u => u.CityID == sm.CityID.Value);
            }
            if (sm.PointsFrom.HasValue)
            {
                q = q.Where(u => (u.RemainedPoints ?? 0) >= sm.PointsFrom.Value);
            }
            if (sm.PointsTo.HasValue)
            {
                q = q.Where(u => (u.RemainedPoints ?? 0) <= sm.PointsTo.Value);
            }
            if (sm.IsEligibleForReward.HasValue)
            {
                q = q.Where(u => u.IsEligibleForReward == sm.IsEligibleForReward.Value);
            }
            if (sm.HasReceivedReward.HasValue)
            {
                q = q.Where(u => u.HasReceivedReward == sm.HasReceivedReward.Value);
            }
            if (sm.CardFrom.HasValue || sm.CardTo.HasValue)
            {
                var from = sm.CardFrom ?? DateTime.MinValue;
                var to = sm.CardTo?.Date.AddDays(1) ?? DateTime.MaxValue;
                q = q.Where(u => u.CardRegistrations.Any(c => c.CreatedAt >= from && c.CreatedAt < to));
            }

            if (sm.PageSize <= 0)
                sm.PageSize = 10;

            sm.RecordCount = await q.CountAsync();
            var pageCount = sm.PageCount;
            var pageIndex = sm.PageIndex < 0 ? 0 : sm.PageIndex;
            if (pageCount > 0 && pageIndex >= pageCount)
                pageIndex = pageCount - 1;
            sm.PageIndex = pageIndex;

            var users = await q
                .OrderByDescending(u => u.RegisterDate)
                .Skip(pageIndex * sm.PageSize)
                .Take(sm.PageSize)
                .Select(UserMapper.ToListItem)
                .ToListAsync();

            return new UserListComplexModel { userList = users, sm = sm };
        }

        public Task<ApplicationUser?> GetByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return Task.FromResult<ApplicationUser?>(null);

            return userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phone);
        }

        public async Task<OperationResult> CreateCustomer(string phone, string? firstName, string? lastName)
        {
            var op = new OperationResult("CreateCustomer");
            if (string.IsNullOrWhiteSpace(phone))
                return op.ToFailed("شماره موبایل اجباری است");

            var user = new ApplicationUser
            {
                UserName = phone,
                PhoneNumber = phone,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsActive = true,
                IsConfirmedCode = true
            };

            var tempPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12)) + "Aa1";
            var createResult = await userManager.CreateAsync(user, tempPassword);
            if (!createResult.Succeeded)
            {
                return op.ToFailed(string.Join(" | ", createResult.Errors.Select(x => x.Description)));
            }

            return op.ToSuccess("کاربر ایجاد شد");
        }

        public async Task EnsureCustomerRole(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;

            if (!await roleManager.RoleExistsAsync("Customer"))
                await roleManager.CreateAsync(new IdentityRole("Customer"));

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return;

            if (!await userManager.IsInRoleAsync(user, "Customer"))
                await userManager.AddToRoleAsync(user, "Customer");
        }

    }
}

