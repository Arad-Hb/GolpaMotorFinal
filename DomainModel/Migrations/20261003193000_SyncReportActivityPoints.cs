using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainModel.Migrations
{
    /// <inheritdoc />
    public partial class SyncReportActivityPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM ReportActivityLogs;

                SELECT
                    regs.CardRegisterationID,
                    txs.PointTransactionID
                INTO #Pairs
                FROM
                (
                    SELECT
                        cr.CardRegisterationID,
                        cr.UserID,
                        CASE WHEN cr.EarnedPionts <> 0 THEN cr.EarnedPionts ELSE p.ProductPoint END AS Points,
                        ROW_NUMBER() OVER (
                            PARTITION BY cr.UserID, CASE WHEN cr.EarnedPionts <> 0 THEN cr.EarnedPionts ELSE p.ProductPoint END
                            ORDER BY cr.CreatedAt, cr.CardRegisterationID) AS PairNumber
                    FROM CardRegistrations cr
                    INNER JOIN WarrantyCards wc ON wc.WarrantyCardID = cr.WarrantyCardID
                    INNER JOIN Products p ON p.ProductID = wc.ProductID
                ) regs
                INNER JOIN
                (
                    SELECT
                        pt.PointTransactionID,
                        pt.UserID,
                        pt.PointsAmount,
                        ROW_NUMBER() OVER (
                            PARTITION BY pt.UserID, pt.PointsAmount
                            ORDER BY COALESCE(pt.PointTransactionDate, CONVERT(datetime2, '19000101', 112)), pt.PointTransactionID) AS PairNumber
                    FROM PointTransactions pt
                    WHERE pt.PointsAmount > 0
                      AND pt.RewardRequestID IS NULL
                ) txs
                    ON txs.UserID = regs.UserID
                   AND txs.PointsAmount = regs.Points
                   AND txs.PairNumber = regs.PairNumber;

                CREATE TABLE #Events
                (
                    SourceKey nvarchar(100) NOT NULL,
                    ActivityType nvarchar(40) NOT NULL,
                    OccurredAtUtc datetime2 NOT NULL,
                    UserID nvarchar(450) NOT NULL,
                    ProductID bigint NULL,
                    WarrantyCardID bigint NULL,
                    CardRegistrationID int NULL,
                    RewardRequestID int NULL,
                    PointTransactionID int NULL,
                    PointsDelta int NOT NULL,
                    PendingDelta int NOT NULL,
                    StatusTitle nvarchar(100) NULL,
                    Description nvarchar(500) NULL,
                    SortRank int NOT NULL
                );

                INSERT INTO #Events
                (
                    SourceKey, ActivityType, OccurredAtUtc, UserID, ProductID,
                    WarrantyCardID, CardRegistrationID, PointTransactionID,
                    PointsDelta, PendingDelta, StatusTitle, Description, SortRank
                )
                SELECT
                    CONCAT('registration:', cr.CardRegisterationID),
                    'CardRegistered',
                    cr.CreatedAt,
                    cr.UserID,
                    wc.ProductID,
                    wc.WarrantyCardID,
                    cr.CardRegisterationID,
                    pairs.PointTransactionID,
                    CASE WHEN cr.EarnedPionts <> 0 THEN cr.EarnedPionts ELSE p.ProductPoint END,
                    0,
                    N'فعال شده',
                    LEFT(CONCAT(N'ثبت کارت گارانتی ', wc.SerialNumber), 500),
                    1
                FROM CardRegistrations cr
                INNER JOIN WarrantyCards wc ON wc.WarrantyCardID = cr.WarrantyCardID
                INNER JOIN Products p ON p.ProductID = wc.ProductID
                LEFT JOIN #Pairs pairs ON pairs.CardRegisterationID = cr.CardRegisterationID;

                INSERT INTO #Events
                (
                    SourceKey, ActivityType, OccurredAtUtc, UserID,
                    RewardRequestID, PointsDelta, PendingDelta,
                    StatusTitle, Description, SortRank
                )
                SELECT
                    CONCAT('reward-request:', rr.RewardRequestID),
                    'RewardRequested',
                    COALESCE(rr.RequestDate, SYSUTCDATETIME()),
                    rr.UserID,
                    rr.RewardRequestID,
                    0,
                    rc.RequiredPoints,
                    N'در انتظار بررسی',
                    LEFT(CONCAT(N'درخواست پاداش: ', rc.Title), 500),
                    3
                FROM RewardRequests rr
                INNER JOIN RewardCatalogs rc ON rc.RewardCatalogID = rr.RewardCatalogID;

                INSERT INTO #Events
                (
                    SourceKey, ActivityType, OccurredAtUtc, UserID,
                    RewardRequestID, PointTransactionID, PointsDelta, PendingDelta,
                    StatusTitle, Description, SortRank
                )
                SELECT
                    CONCAT('reward-status:', rr.RewardRequestID),
                    CASE
                        WHEN status.Title IN (N'تایید شده', N'پرداخت شده') THEN 'RewardApproved'
                        ELSE 'RewardRejected'
                    END,
                    rr.ReviewedDate,
                    rr.UserID,
                    rr.RewardRequestID,
                    pointRow.PointTransactionID,
                    CASE
                        WHEN status.Title IN (N'تایید شده', N'پرداخت شده')
                            THEN COALESCE(pointRow.PointsAmount, -rc.RequiredPoints)
                        ELSE 0
                    END,
                    -rc.RequiredPoints,
                    status.Title,
                    LEFT(CONCAT(N'درخواست پاداش: ', rc.Title), 500),
                    4
                FROM RewardRequests rr
                INNER JOIN RewardCatalogs rc ON rc.RewardCatalogID = rr.RewardCatalogID
                INNER JOIN RewardDeliveryStatuses status
                    ON status.RewardDeliveryStatusID = rr.RewardDeliveryStatusID
                OUTER APPLY
                (
                    SELECT TOP (1) pt.PointTransactionID, pt.PointsAmount
                    FROM PointTransactions pt
                    WHERE pt.RewardRequestID = rr.RewardRequestID
                      AND pt.PointsAmount < 0
                    ORDER BY pt.PointTransactionID DESC
                ) pointRow
                WHERE rr.ReviewedDate IS NOT NULL
                  AND status.Title IN (N'تایید شده', N'پرداخت شده', N'رد شده', N'لغو شده');

                INSERT INTO #Events
                (
                    SourceKey, ActivityType, OccurredAtUtc, UserID,
                    PointTransactionID, PointsDelta, PendingDelta,
                    StatusTitle, Description, SortRank
                )
                SELECT
                    CONCAT('adjustment:', pt.PointTransactionID),
                    'BalanceAdjustment',
                    COALESCE(pt.PointTransactionDate, SYSUTCDATETIME()),
                    pt.UserID,
                    pt.PointTransactionID,
                    pt.PointsAmount,
                    0,
                    N'اصلاح امتیاز',
                    LEFT(COALESCE(pt.Description, N'اصلاح امتیاز'), 500),
                    2
                FROM PointTransactions pt
                WHERE pt.PointsAmount <> 0
                  AND pt.RewardRequestID IS NULL
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM #Pairs pairs
                      WHERE pairs.PointTransactionID = pt.PointTransactionID
                  );

                INSERT INTO ReportActivityLogs
                (
                    SourceKey, ActivityType, OccurredAtUtc, UserID, ProductID,
                    WarrantyCardID, CardRegistrationID, RewardRequestID, PointTransactionID,
                    PointsDelta, TotalEarnedPoints, TotalSettledPoints,
                    RemainedPoints, AvailablePoints, StatusTitle, Description
                )
                SELECT
                    e.SourceKey,
                    e.ActivityType,
                    e.OccurredAtUtc,
                    e.UserID,
                    e.ProductID,
                    e.WarrantyCardID,
                    e.CardRegistrationID,
                    e.RewardRequestID,
                    e.PointTransactionID,
                    e.PointsDelta,
                    balances.Earned,
                    balances.Settled,
                    balances.Earned - balances.Settled,
                    CASE
                        WHEN balances.Earned - balances.Settled - balances.Pending < 0 THEN 0
                        ELSE balances.Earned - balances.Settled - balances.Pending
                    END,
                    e.StatusTitle,
                    e.Description
                FROM #Events e
                CROSS APPLY
                (
                    SELECT
                        SUM(CASE WHEN earlier.PointsDelta > 0 THEN earlier.PointsDelta ELSE 0 END) AS Earned,
                        SUM(CASE WHEN earlier.PointsDelta < 0 THEN -earlier.PointsDelta ELSE 0 END) AS Settled,
                        SUM(earlier.PendingDelta) AS Pending
                    FROM #Events earlier
                    WHERE earlier.UserID = e.UserID
                      AND (
                          earlier.OccurredAtUtc < e.OccurredAtUtc
                          OR (earlier.OccurredAtUtc = e.OccurredAtUtc AND earlier.SortRank < e.SortRank)
                          OR (earlier.OccurredAtUtc = e.OccurredAtUtc AND earlier.SortRank = e.SortRank AND earlier.SourceKey <= e.SourceKey)
                      )
                ) balances;

                INSERT INTO ReportActivityLogs
                (
                    SourceKey, ActivityType, OccurredAtUtc, UserID,
                    PointsDelta, TotalEarnedPoints, TotalSettledPoints,
                    RemainedPoints, AvailablePoints, StatusTitle, Description
                )
                SELECT
                    CONCAT('adjustment:close:', u.Id),
                    'BalanceAdjustment',
                    DATEADD(second, 1, COALESCE(
                        (SELECT MAX(e.OccurredAtUtc) FROM #Events e WHERE e.UserID = u.Id),
                        SYSUTCDATETIME())),
                    u.Id,
                    shown.Remained - (ledger.Earned - ledger.Settled),
                    shown.Earned,
                    shown.Settled,
                    shown.Remained,
                    CASE
                        WHEN shown.Remained - ledger.Pending < 0 THEN 0
                        ELSE shown.Remained - ledger.Pending
                    END,
                    N'اصلاح مانده',
                    N'همگام‌سازی مانده با حساب کاربر'
                FROM AspNetUsers u
                CROSS APPLY
                (
                    SELECT
                        COALESCE(SUM(CASE WHEN e.PointsDelta > 0 THEN e.PointsDelta ELSE 0 END), 0) AS Earned,
                        COALESCE(SUM(CASE WHEN e.PointsDelta < 0 THEN -e.PointsDelta ELSE 0 END), 0) AS Settled,
                        COALESCE(SUM(e.PendingDelta), 0) AS Pending
                    FROM #Events e
                    WHERE e.UserID = u.Id
                ) ledger
                CROSS APPLY
                (
                    SELECT
                        COALESCE(u.TotalEarnedPoints, ledger.Earned) AS Earned,
                        COALESCE(u.TotalSettledPoints, ledger.Settled) AS Settled,
                        COALESCE(
                            u.RemainedPoints,
                            COALESCE(u.TotalEarnedPoints, ledger.Earned) - COALESCE(u.TotalSettledPoints, ledger.Settled)) AS Remained
                ) shown
                WHERE (u.TotalEarnedPoints IS NOT NULL
                       OR u.TotalSettledPoints IS NOT NULL
                       OR u.RemainedPoints IS NOT NULL)
                  AND (shown.Earned <> ledger.Earned
                       OR shown.Settled <> ledger.Settled
                       OR shown.Remained <> (ledger.Earned - ledger.Settled));

                UPDATE u
                SET
                    TotalEarnedPoints = COALESCE(u.TotalEarnedPoints, COALESCE(latest.TotalEarnedPoints, 0)),
                    TotalSettledPoints = COALESCE(u.TotalSettledPoints, COALESCE(latest.TotalSettledPoints, 0)),
                    RemainedPoints = COALESCE(u.RemainedPoints, COALESCE(latest.RemainedPoints, 0)),
                    TotalRegisteredCards = COALESCE(u.TotalRegisteredCards, COALESCE(cards.RegistrationCount, 0))
                FROM AspNetUsers u
                OUTER APPLY
                (
                    SELECT TOP (1)
                        l.TotalEarnedPoints,
                        l.TotalSettledPoints,
                        l.RemainedPoints
                    FROM ReportActivityLogs l
                    WHERE l.UserID = u.Id
                    ORDER BY l.OccurredAtUtc DESC, l.ReportActivityLogID DESC
                ) latest
                OUTER APPLY
                (
                    SELECT COUNT(*) AS RegistrationCount
                    FROM CardRegistrations cr
                    WHERE cr.UserID = u.Id
                ) cards
                WHERE u.TotalEarnedPoints IS NULL
                   OR u.TotalSettledPoints IS NULL
                   OR u.RemainedPoints IS NULL
                   OR u.TotalRegisteredCards IS NULL;

                DROP TABLE #Events;
                DROP TABLE #Pairs;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
