using DataAccess.Services;
using DomainModel.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class ReportActivityWriter : IReportActivityWriter
    {
        private readonly GolpaMotorDbContext db;
        private readonly List<ReportActivityLog> pendingKeys = new();

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
            var cardLog = new ReportActivityLog
            {
                SourceKey = registration.CardRegisterationID > 0
                    ? $"registration:{registration.CardRegisterationID}"
                    : $"registration:card:{card.WarrantyCardID}",
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
            };
            pendingKeys.Add(cardLog);
            db.ReportActivityLogs.Add(cardLog);
        }

        public async Task AddRewardActivityAsync(
            RewardRequest request,
            string activityType,
            PointTransaction? transaction = null)
        {
            var balances = await GetBalancesAsync(request.UserID);

            var rewardLog = new ReportActivityLog
            {
                SourceKey = StableRewardKey(request, activityType),
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
            };
            pendingKeys.Add(rewardLog);
            db.ReportActivityLogs.Add(rewardLog);
        }

        public async Task FinalizeSourceKeysAsync()
        {
            foreach (var log in pendingKeys)
            {
                var key = StableKey(log);
                if (key != null && log.SourceKey != key)
                    log.SourceKey = key;
            }

            pendingKeys.Clear();
            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync();
        }

        private static string StableRewardKey(RewardRequest request, string activityType)
        {
            if (request.RewardRequestID <= 0)
                return $"reward-request:new:{Guid.NewGuid():N}";

            return activityType == ReportActivityTypes.RewardRequested
                ? $"reward-request:{request.RewardRequestID}"
                : $"reward-status:{request.RewardRequestID}";
        }

        private static string? StableKey(ReportActivityLog log)
        {
            if (log.ActivityType == ReportActivityTypes.CardRegistered && log.CardRegistrationID is > 0)
                return $"registration:{log.CardRegistrationID.Value}";

            if (log.ActivityType == ReportActivityTypes.RewardRequested && log.RewardRequestID is > 0)
                return $"reward-request:{log.RewardRequestID.Value}";

            if ((log.ActivityType == ReportActivityTypes.RewardApproved ||
                 log.ActivityType == ReportActivityTypes.RewardRejected) &&
                log.RewardRequestID is > 0)
                return $"reward-status:{log.RewardRequestID.Value}";

            return null;
        }

        private async Task<(int Earned, int Settled, int Remained, int Available)> GetBalancesAsync(string userId)
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
            return (earned, settled, remained, remained);
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
