using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainModel.Migrations
{
    /// <inheritdoc />
    public partial class AddUnifiedReportActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WarrantyCards_ProductID",
                table: "WarrantyCards");

            migrationBuilder.AddColumn<DateTime>(
                name: "IssuedAtUtc",
                table: "WarrantyCards",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProductAssignedAtUtc",
                table: "WarrantyCards",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.CreateTable(
                name: "ReportActivityLogs",
                columns: table => new
                {
                    ReportActivityLogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProductID = table.Column<long>(type: "bigint", nullable: true),
                    WarrantyCardID = table.Column<long>(type: "bigint", nullable: true),
                    CardRegistrationID = table.Column<int>(type: "int", nullable: true),
                    RewardRequestID = table.Column<int>(type: "int", nullable: true),
                    PointTransactionID = table.Column<int>(type: "int", nullable: true),
                    PointsDelta = table.Column<int>(type: "int", nullable: false),
                    TotalEarnedPoints = table.Column<int>(type: "int", nullable: false),
                    TotalSettledPoints = table.Column<int>(type: "int", nullable: false),
                    RemainedPoints = table.Column<int>(type: "int", nullable: false),
                    AvailablePoints = table.Column<int>(type: "int", nullable: false),
                    StatusTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportActivityLogs", x => x.ReportActivityLogID);
                    table.ForeignKey(
                        name: "FK_ReportActivityLogs_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportActivityLogs_CardRegistrations_CardRegistrationID",
                        column: x => x.CardRegistrationID,
                        principalTable: "CardRegistrations",
                        principalColumn: "CardRegisterationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportActivityLogs_PointTransactions_PointTransactionID",
                        column: x => x.PointTransactionID,
                        principalTable: "PointTransactions",
                        principalColumn: "PointTransactionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportActivityLogs_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportActivityLogs_RewardRequests_RewardRequestID",
                        column: x => x.RewardRequestID,
                        principalTable: "RewardRequests",
                        principalColumn: "RewardRequestID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportActivityLogs_WarrantyCards_WarrantyCardID",
                        column: x => x.WarrantyCardID,
                        principalTable: "WarrantyCards",
                        principalColumn: "WarrantyCardID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(
                """
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_WarrantyCards_SerialNumber'
                      AND object_id = OBJECT_ID(N'[WarrantyCards]')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_WarrantyCards_SerialNumber]
                    ON [WarrantyCards] ([SerialNumber]);
                END;

                UPDATE p
                SET CreatedAtUtc = COALESCE(
                    (SELECT MIN(cr.CreatedAt)
                     FROM WarrantyCards wc
                     INNER JOIN CardRegistrations cr ON cr.WarrantyCardID = wc.WarrantyCardID
                     WHERE wc.ProductID = p.ProductID),
                    CreatedAtUtc)
                FROM Products p;

                UPDATE wc
                SET IssuedAtUtc = COALESCE(
                        (SELECT MIN(cr.CreatedAt)
                         FROM CardRegistrations cr
                         WHERE cr.WarrantyCardID = wc.WarrantyCardID),
                        p.CreatedAtUtc),
                    ProductAssignedAtUtc = COALESCE(
                        (SELECT MIN(cr.CreatedAt)
                         FROM CardRegistrations cr
                         WHERE cr.WarrantyCardID = wc.WarrantyCardID),
                        p.CreatedAtUtc)
                FROM WarrantyCards wc
                INNER JOIN Products p ON p.ProductID = wc.ProductID;

                INSERT INTO ReportActivityLogs
                (
                    SourceKey, ActivityType, OccurredAtUtc, UserID, ProductID,
                    WarrantyCardID, CardRegistrationID, PointTransactionID,
                    PointsDelta, TotalEarnedPoints, TotalSettledPoints,
                    RemainedPoints, AvailablePoints, StatusTitle, Description
                )
                SELECT
                    CONCAT('registration:', cr.CardRegisterationID),
                    'CardRegistered',
                    cr.CreatedAt,
                    cr.UserID,
                    wc.ProductID,
                    wc.WarrantyCardID,
                    cr.CardRegisterationID,
                    matched.PointTransactionID,
                    CASE WHEN cr.EarnedPionts <> 0 THEN cr.EarnedPionts ELSE p.ProductPoint END,
                    balances.Earned,
                    balances.Settled,
                    balances.Earned - balances.Settled,
                    balances.Earned - balances.Settled,
                    N'فعال شده',
                    CONCAT(N'ثبت کارت گارانتی ', wc.SerialNumber)
                FROM CardRegistrations cr
                INNER JOIN WarrantyCards wc ON wc.WarrantyCardID = cr.WarrantyCardID
                INNER JOIN Products p ON p.ProductID = wc.ProductID
                OUTER APPLY
                (
                    SELECT TOP (1) pt.PointTransactionID
                    FROM PointTransactions pt
                    WHERE pt.UserID = cr.UserID
                      AND pt.PointsAmount > 0
                      AND ABS(DATEDIFF(SECOND, pt.PointTransactionDate, cr.CreatedAt)) <= 5
                    ORDER BY pt.PointTransactionID
                ) matched
                OUTER APPLY
                (
                    SELECT
                        COALESCE(SUM(CASE WHEN pt.PointsAmount > 0 THEN pt.PointsAmount ELSE 0 END), 0) AS Earned,
                        COALESCE(SUM(CASE WHEN pt.PointsAmount < 0 THEN -pt.PointsAmount ELSE 0 END), 0) AS Settled
                    FROM PointTransactions pt
                    WHERE pt.UserID = cr.UserID
                      AND COALESCE(pt.PointTransactionDate, cr.CreatedAt) <= cr.CreatedAt
                ) balances
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM ReportActivityLogs l
                    WHERE l.SourceKey = CONCAT('registration:', cr.CardRegisterationID)
                );

                INSERT INTO ReportActivityLogs
                (
                    SourceKey, ActivityType, OccurredAtUtc, UserID,
                    RewardRequestID, PointsDelta, TotalEarnedPoints,
                    TotalSettledPoints, RemainedPoints, AvailablePoints,
                    StatusTitle, Description
                )
                SELECT
                    CONCAT('reward-request:', rr.RewardRequestID),
                    'RewardRequested',
                    COALESCE(rr.RequestDate, SYSUTCDATETIME()),
                    rr.UserID,
                    rr.RewardRequestID,
                    0,
                    balances.Earned,
                    balances.Settled,
                    balances.Earned - balances.Settled,
                    balances.Earned - balances.Settled - rc.RequiredPoints,
                    N'در انتظار بررسی',
                    CONCAT(N'درخواست پاداش: ', rc.Title)
                FROM RewardRequests rr
                INNER JOIN RewardCatalogs rc ON rc.RewardCatalogID = rr.RewardCatalogID
                OUTER APPLY
                (
                    SELECT
                        COALESCE(SUM(CASE WHEN pt.PointsAmount > 0 THEN pt.PointsAmount ELSE 0 END), 0) AS Earned,
                        COALESCE(SUM(CASE WHEN pt.PointsAmount < 0 THEN -pt.PointsAmount ELSE 0 END), 0) AS Settled
                    FROM PointTransactions pt
                    WHERE pt.UserID = rr.UserID
                      AND COALESCE(pt.PointTransactionDate, rr.RequestDate) <= rr.RequestDate
                ) balances
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM ReportActivityLogs l
                    WHERE l.SourceKey = CONCAT('reward-request:', rr.RewardRequestID)
                );

                INSERT INTO ReportActivityLogs
                (
                    SourceKey, ActivityType, OccurredAtUtc, UserID,
                    RewardRequestID, PointTransactionID, PointsDelta,
                    TotalEarnedPoints, TotalSettledPoints, RemainedPoints,
                    AvailablePoints, StatusTitle, Description
                )
                SELECT
                    CONCAT('reward-status:', rr.RewardRequestID),
                    CASE WHEN status.Title = N'رد شده' THEN 'RewardRejected' ELSE 'RewardApproved' END,
                    rr.ReviewedDate,
                    rr.UserID,
                    rr.RewardRequestID,
                    pointRow.PointTransactionID,
                    COALESCE(pointRow.PointsAmount, 0),
                    balances.Earned,
                    balances.Settled,
                    balances.Earned - balances.Settled,
                    balances.Earned - balances.Settled,
                    status.Title,
                    CONCAT(N'درخواست پاداش: ', rc.Title)
                FROM RewardRequests rr
                INNER JOIN RewardCatalogs rc ON rc.RewardCatalogID = rr.RewardCatalogID
                INNER JOIN RewardDeliveryStatuses status
                    ON status.RewardDeliveryStatusID = rr.RewardDeliveryStatusID
                OUTER APPLY
                (
                    SELECT TOP (1) pt.PointTransactionID, pt.PointsAmount
                    FROM PointTransactions pt
                    WHERE pt.RewardRequestID = rr.RewardRequestID
                    ORDER BY pt.PointTransactionID DESC
                ) pointRow
                OUTER APPLY
                (
                    SELECT
                        COALESCE(SUM(CASE WHEN pt.PointsAmount > 0 THEN pt.PointsAmount ELSE 0 END), 0) AS Earned,
                        COALESCE(SUM(CASE WHEN pt.PointsAmount < 0 THEN -pt.PointsAmount ELSE 0 END), 0) AS Settled
                    FROM PointTransactions pt
                    WHERE pt.UserID = rr.UserID
                      AND COALESCE(pt.PointTransactionDate, rr.ReviewedDate) <= rr.ReviewedDate
                ) balances
                WHERE rr.ReviewedDate IS NOT NULL
                  AND status.Title IN (N'تایید شده', N'رد شده')
                  AND NOT EXISTS
                  (
                      SELECT 1 FROM ReportActivityLogs l
                      WHERE l.SourceKey = CONCAT('reward-status:', rr.RewardRequestID)
                  );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCards_ProductID_IssuedAtUtc",
                table: "WarrantyCards",
                columns: new[] { "ProductID", "IssuedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCards_ProductID_ProductAssignedAtUtc",
                table: "WarrantyCards",
                columns: new[] { "ProductID", "ProductAssignedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedAtUtc",
                table: "Products",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ReportActivityLogs_ActivityType_OccurredAtUtc",
                table: "ReportActivityLogs",
                columns: new[] { "ActivityType", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportActivityLogs_CardRegistrationID",
                table: "ReportActivityLogs",
                column: "CardRegistrationID");

            migrationBuilder.CreateIndex(
                name: "IX_ReportActivityLogs_PointTransactionID",
                table: "ReportActivityLogs",
                column: "PointTransactionID");

            migrationBuilder.CreateIndex(
                name: "IX_ReportActivityLogs_ProductID_OccurredAtUtc",
                table: "ReportActivityLogs",
                columns: new[] { "ProductID", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportActivityLogs_RewardRequestID_OccurredAtUtc",
                table: "ReportActivityLogs",
                columns: new[] { "RewardRequestID", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportActivityLogs_SourceKey",
                table: "ReportActivityLogs",
                column: "SourceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportActivityLogs_UserID_OccurredAtUtc",
                table: "ReportActivityLogs",
                columns: new[] { "UserID", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportActivityLogs_WarrantyCardID_OccurredAtUtc",
                table: "ReportActivityLogs",
                columns: new[] { "WarrantyCardID", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyCards_ProductID_IssuedAtUtc",
                table: "WarrantyCards");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyCards_ProductID_ProductAssignedAtUtc",
                table: "WarrantyCards");

            migrationBuilder.DropIndex(
                name: "IX_Products_CreatedAtUtc",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IssuedAtUtc",
                table: "WarrantyCards");

            migrationBuilder.DropColumn(
                name: "ProductAssignedAtUtc",
                table: "WarrantyCards");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCards_ProductID",
                table: "WarrantyCards",
                column: "ProductID");
        }
    }
}
