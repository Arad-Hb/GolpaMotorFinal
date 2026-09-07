using DomainModel.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Helpers
{
    public static class RewardEligibilityHelper
    {
        public static async Task<int?> GetMinRequiredPointsAsync(GolpaMotorDbContext db)
        {
            return await db.RewardCatalogs
                .Where(x => x.IsActive)
                .Select(x => (int?)x.RequiredPoints)
                .MinAsync();
        }

        public static async Task ApplyToUserAsync(GolpaMotorDbContext db, ApplicationUser user)
        {
            var minRequired = await GetMinRequiredPointsAsync(db);
            var remained = user.RemainedPoints ?? 0;
            user.IsEligibleForReward = minRequired.HasValue && remained >= minRequired.Value;
        }

        public static async Task RefreshUserAsync(GolpaMotorDbContext db, string userId, bool saveChanges = true)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);
            if (user == null)
                return;

            var earned = await db.PointTransactions
                .Where(x => x.UserID == userId && x.PointsAmount > 0)
                .SumAsync(x => (int?)x.PointsAmount) ?? 0;
            var settled = Math.Abs(await db.PointTransactions
                .Where(x => x.UserID == userId && x.PointsAmount < 0)
                .SumAsync(x => (int?)x.PointsAmount) ?? 0);

            user.TotalEarnedPoints = earned;
            user.TotalSettledPoints = settled;
            user.RemainedPoints = earned - settled;

            await ApplyToUserAsync(db, user);

            if (saveChanges)
                await db.SaveChangesAsync();
        }

        public static async Task RefreshAllUsersAsync(GolpaMotorDbContext db, bool saveChanges = true)
        {
            var minRequired = await GetMinRequiredPointsAsync(db);
            var users = await db.Users.Where(x => !x.IsDeleted).ToListAsync();
            foreach (var user in users)
            {
                var remained = user.RemainedPoints ?? 0;
                user.IsEligibleForReward = minRequired.HasValue && remained >= minRequired.Value;
            }

            if (saveChanges)
                await db.SaveChangesAsync();
        }

        public static async Task<int> GetStatusIdAsync(GolpaMotorDbContext db, string title)
        {
            return await db.RewardDeliveryStatuses
                .Where(x => x.Title == title)
                .Select(x => x.RewardDeliveryStatusID)
                .FirstOrDefaultAsync();
        }
    }
}
