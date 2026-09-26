using DataAccess.Services;
using DomainModel.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class ReportActivityWriter : IReportActivityWriter
    {
        private readonly GolpaMotorDbContext db;

        public ReportActivityWriter(GolpaMotorDbContext db)
        {
            this.db = db;
        }

        public async Task AddCardRegisteredAsync(
            string userId,
            WarrantyCard card,
            CardRegistration registration,
            PointTransaction transaction)
        {
            var balances = await GetBalancesAsync(userId);
            transaction.PointsBeforeTransaction = balances.Remained - transaction.PointsAmount;
            transaction.PointsAfterTransaction = balances.Remained;
            db.ReportActivityLogs.Add(new ReportActivityLog
            {
                SourceKey = $"live:{Guid.NewGuid():N}",
                ActivityType = ReportActivityTypes.CardRegistered,
                OccurredAtUtc = registration.CreatedAt,
                UserID = userId,
                ProductID = card.ProductID,
                WarrantyCard = card,
                CardRegistration = registration,
                PointTransaction = transaction,
                PointsDelta = transaction.PointsAmount,
                TotalEarnedPoints = balances.Earned,
                TotalSettledPoints = balances.Settled,
                RemainedPoints = balances.Remained,
                AvailablePoints = balances.Available,
                StatusTitle = "فعال شده",
                Description = $"ثبت کارت گارانتی {card.SerialNumber}"
            });
        }

        public async Task AddRewardActivityAsync(
            RewardRequest request,
            string activityType,
            PointTransaction? transaction = null)
        {
            var balances = await GetBalancesAsync(
                request.UserID,
                request.RewardRequestID > 0 ? request.RewardRequestID : null,
                activityType == ReportActivityTypes.RewardRequested
                    ? request.RewardCatalog?.RequiredPoints ?? 0
                    : 0);

            db.ReportActivityLogs.Add(new ReportActivityLog
            {
                SourceKey = $"live:{Guid.NewGuid():N}",
                ActivityType = activityType,
                OccurredAtUtc = activityType == ReportActivityTypes.RewardRequested
                    ? ToUtc(request.RequestDate)
                    : ToUtc(request.ReviewedDate),
                UserID = request.UserID,
                RewardRequest = request,
                PointTransaction = transaction,
                PointsDelta = transaction?.PointsAmount ?? 0,
                TotalEarnedPoints = balances.Earned,
                TotalSettledPoints = balances.Settled,
                RemainedPoints = balances.Remained,
                AvailablePoints = balances.Available,
                StatusTitle = activityType switch
                {
                    ReportActivityTypes.RewardRequested => RewardStatusTitles.Pending,
                    ReportActivityTypes.RewardApproved => RewardStatusTitles.Approved,
                    ReportActivityTypes.RewardRejected => RewardStatusTitles.Rejected,
                    _ => request.RewardDeliveryStatus?.Title
                },
                Description = request.RewardCatalog == null
                    ? "درخواست پاداش"
                    : $"درخواست پاداش: {request.RewardCatalog.Title}"
            });
        }

        private async Task<(int Earned, int Settled, int Remained, int Available)> GetBalancesAsync(
            string userId,
            int? excludePendingRequestId = null,
            int additionalLockedPoints = 0)
        {
            var persisted = await db.PointTransactions
                .Where(x => x.UserID == userId)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Earned = g.Sum(x => x.PointsAmount > 0 ? x.PointsAmount : 0),
                    Settled = g.Sum(x => x.PointsAmount < 0 ? -x.PointsAmount : 0)
                })
                .FirstOrDefaultAsync();

            var added = db.ChangeTracker.Entries<PointTransaction>()
                .Where(x => x.State == EntityState.Added && x.Entity.UserID == userId)
                .Select(x => x.Entity.PointsAmount)
                .ToList();

            var earned = (persisted?.Earned ?? 0) + added.Where(x => x > 0).Sum();
            var settled = (persisted?.Settled ?? 0) + added.Where(x => x < 0).Sum(x => -x);
            var remained = earned - settled;

            var pendingTitle = RewardStatusTitles.Pending;
            var locked = await db.RewardRequests
                .Where(x =>
                    x.UserID == userId &&
                    !x.IsComplete &&
                    x.RewardDeliveryStatus.Title == pendingTitle &&
                    (!excludePendingRequestId.HasValue || x.RewardRequestID != excludePendingRequestId.Value))
                .SumAsync(x => (int?)x.RewardCatalog.RequiredPoints) ?? 0;

            return (earned, settled, remained, Math.Max(0, remained - locked - additionalLockedPoints));
        }

        private static DateTime ToUtc(DateTime? value)
        {
            if (!value.HasValue)
                return DateTime.UtcNow;
            if (value.Value.Kind == DateTimeKind.Utc)
                return value.Value;
            return value.Value.ToUniversalTime();
        }
    }
}
