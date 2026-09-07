using DataAccess.Helpers;
using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Reward;
using Framework.Common;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class RewardRequestRepository : IRewardRequestRepository
    {
        private readonly GolpaMotorDbContext db;

        public RewardRequestRepository(GolpaMotorDbContext db)
        {
            this.db = db;
        }

        public async Task RefreshEligibility(string userId)
        {
            await RewardEligibilityHelper.RefreshUserAsync(db, userId);
        }

        public async Task<List<UserEligibleRewardItem>> GetEligibleCatalogsForUser(string userId)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);
            if (user == null)
                return new List<UserEligibleRewardItem>();

            var remained = user.RemainedPoints ?? 0;
            var pendingStatusId = await RewardEligibilityHelper.GetStatusIdAsync(db, RewardStatusTitles.Pending);

            return await db.RewardCatalogs
                .Where(x => x.IsActive && x.RequiredPoints <= remained)
                .OrderBy(x => x.RequiredPoints)
                .Select(x => new UserEligibleRewardItem
                {
                    RewardCatalogID = x.RewardCatalogID,
                    Title = x.Title,
                    Description = x.Description,
                    RequiredPoints = x.RequiredPoints,
                    IsCashReward = x.IsCashReward,
                    CashValue = x.CashValue,
                    HasPendingRequest = db.RewardRequests.Any(r =>
                        r.UserID == userId &&
                        r.RewardCatalogID == x.RewardCatalogID &&
                        !r.IsComplete &&
                        r.RewardDeliveryStatusID == pendingStatusId)
                })
                .ToListAsync();
        }

        public async Task<List<RewardRequestListItem>> GetUserRequests(string userId)
        {
            return await ProjectRequests(db.RewardRequests.Where(x => x.UserID == userId))
                .OrderByDescending(x => x.RequestDate)
                .ToListAsync();
        }

        public async Task<RewardRequestListItem?> GetDetails(int rewardRequestId)
        {
            return await ProjectRequests(db.RewardRequests.Where(x => x.RewardRequestID == rewardRequestId))
                .FirstOrDefaultAsync();
        }

        public async Task<RewardRequestListComplexModel> Search(RewardRequestSearchModel searchModel)
        {
            var result = new RewardRequestListComplexModel();
            var query = db.RewardRequests.AsQueryable();

            if (searchModel.RewardDeliveryStatusID.HasValue && searchModel.RewardDeliveryStatusID.Value > 0)
                query = query.Where(x => x.RewardDeliveryStatusID == searchModel.RewardDeliveryStatusID.Value);

            if (searchModel.IsComplete.HasValue)
                query = query.Where(x => x.IsComplete == searchModel.IsComplete.Value);

            if (!string.IsNullOrWhiteSpace(searchModel.SearchTerm))
            {
                var term = searchModel.SearchTerm.Trim();
                query = query.Where(x =>
                    x.RewardCatalog.Title.Contains(term) ||
                    (x.User.FirstName != null && x.User.FirstName.Contains(term)) ||
                    (x.User.LastName != null && x.User.LastName.Contains(term)) ||
                    (x.User.PhoneNumber != null && x.User.PhoneNumber.Contains(term)));
            }

            var totalCount = await query.CountAsync();
            var pageIndex = searchModel.PageIndex < 0 ? 0 : searchModel.PageIndex;
            var pageSize = searchModel.PageSize <= 0 ? 10 : searchModel.PageSize;

            var list = await ProjectRequests(query)
                .OrderByDescending(x => x.RequestDate)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            result.sm = searchModel;
            result.sm.RecordCount = totalCount;
            result.RequestList = list;
            return result;
        }

        public async Task<OperationResult> CreateRequest(string userId, int rewardCatalogId)
        {
            var op = new OperationResult("Create Reward Request");
            try
            {
                var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);
                if (user == null)
                    return op.ToFailed("کاربر یافت نشد");

                var catalog = await db.RewardCatalogs
                    .FirstOrDefaultAsync(x => x.RewardCatalogID == rewardCatalogId && x.IsActive);
                if (catalog == null)
                    return op.ToFailed("پاداش فعال یافت نشد");

                var remained = user.RemainedPoints ?? 0;
                if (remained < catalog.RequiredPoints)
                    return op.ToFailed("امتیاز شما به حد نصاب این پاداش نرسیده است");

                var pendingStatusId = await RewardEligibilityHelper.GetStatusIdAsync(db, RewardStatusTitles.Pending);
                if (pendingStatusId == 0)
                    return op.ToFailed("وضعیت درخواست پاداش در سیستم تعریف نشده است");

                var hasOpen = await db.RewardRequests.AnyAsync(x =>
                    x.UserID == userId &&
                    x.RewardCatalogID == rewardCatalogId &&
                    !x.IsComplete &&
                    x.RewardDeliveryStatusID == pendingStatusId);

                if (hasOpen)
                    return op.ToFailed("برای این پاداش درخواست در انتظار بررسی دارید");

                var request = new RewardRequest
                {
                    UserID = userId,
                    RewardCatalogID = rewardCatalogId,
                    RequestDate = DateTime.Now,
                    IsComplete = false,
                    RewardDeliveryStatusID = pendingStatusId
                };

                db.RewardRequests.Add(request);
                await db.SaveChangesAsync();
                return op.ToSuccess("درخواست پاداش ثبت شد", request.RewardRequestID);
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در ثبت درخواست: " + ex.Message);
            }
        }

        public async Task<OperationResult> Approve(int rewardRequestId)
        {
            var op = new OperationResult("Approve Reward Request");
            try
            {
                var request = await db.RewardRequests
                    .Include(x => x.RewardCatalog)
                    .Include(x => x.User)
                    .Include(x => x.RewardDeliveryStatus)
                    .FirstOrDefaultAsync(x => x.RewardRequestID == rewardRequestId);

                if (request == null)
                    return op.ToFailed("درخواست یافت نشد");

                if (request.IsComplete || request.RewardDeliveryStatus.Title != RewardStatusTitles.Pending)
                    return op.ToFailed("این درخواست قابل تأیید نیست");

                var user = request.User;
                var required = request.RewardCatalog.RequiredPoints;
                var remained = user.RemainedPoints ?? 0;
                if (remained < required)
                    return op.ToFailed("امتیاز مانده کاربر برای این پاداش کافی نیست");

                var approvedStatusId = await RewardEligibilityHelper.GetStatusIdAsync(db, RewardStatusTitles.Approved);
                if (approvedStatusId == 0)
                    return op.ToFailed("وضعیت تأیید در سیستم تعریف نشده است");

                var before = remained;
                var after = remained - required;

                db.PointTransactions.Add(new PointTransaction
                {
                    UserID = user.Id,
                    RewardRequestID = request.RewardRequestID,
                    RewardDeliveryStatusID = approvedStatusId,
                    PointsAmount = -required,
                    PointsBeforeTransaction = before,
                    PointsAfterTransaction = after,
                    PointTransactionDate = DateTime.Now,
                    Description = $"کسر بابت دریافت جایزه: {request.RewardCatalog.Title}"
                });

                request.IsComplete = true;
                request.ReviewedDate = DateTime.Now;
                request.RewardDeliveryStatusID = approvedStatusId;

                user.TotalSettledPoints = (user.TotalSettledPoints ?? 0) + required;
                user.RemainedPoints = after;
                user.HasReceivedReward = true;

                await RewardEligibilityHelper.ApplyToUserAsync(db, user);
                await db.SaveChangesAsync();
                return op.ToSuccess("درخواست تأیید شد و پاداش برای کاربر ثبت شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در تأیید درخواست: " + ex.Message);
            }
        }

        public async Task<OperationResult> Reject(int rewardRequestId)
        {
            var op = new OperationResult("Reject Reward Request");
            try
            {
                var request = await db.RewardRequests
                    .Include(x => x.RewardDeliveryStatus)
                    .FirstOrDefaultAsync(x => x.RewardRequestID == rewardRequestId);

                if (request == null)
                    return op.ToFailed("درخواست یافت نشد");

                if (request.IsComplete || request.RewardDeliveryStatus.Title != RewardStatusTitles.Pending)
                    return op.ToFailed("این درخواست قابل رد نیست");

                var rejectedStatusId = await RewardEligibilityHelper.GetStatusIdAsync(db, RewardStatusTitles.Rejected);
                if (rejectedStatusId == 0)
                    return op.ToFailed("وضعیت رد در سیستم تعریف نشده است");

                request.ReviewedDate = DateTime.Now;
                request.RewardDeliveryStatusID = rejectedStatusId;
                await db.SaveChangesAsync();
                return op.ToSuccess("درخواست رد شد");
            }
            catch (Exception ex)
            {
                return op.ToFailed("خطا در رد درخواست: " + ex.Message);
            }
        }

        private static IQueryable<RewardRequestListItem> ProjectRequests(IQueryable<RewardRequest> query)
        {
            return query.Select(x => new RewardRequestListItem
            {
                RewardRequestID = x.RewardRequestID,
                UserID = x.UserID,
                UserFullName = ((x.User.FirstName ?? "") + " " + (x.User.LastName ?? "")),
                PhoneNumber = x.User.PhoneNumber,
                RewardCatalogID = x.RewardCatalogID,
                CatalogTitle = x.RewardCatalog.Title,
                RequiredPoints = x.RewardCatalog.RequiredPoints,
                RemainedPoints = x.User.RemainedPoints ?? 0,
                RequestDate = x.RequestDate,
                ReviewedDate = x.ReviewedDate,
                IsComplete = x.IsComplete,
                RewardDeliveryStatusID = x.RewardDeliveryStatusID,
                StatusTitle = x.RewardDeliveryStatus.Title
            });
        }
    }
}
