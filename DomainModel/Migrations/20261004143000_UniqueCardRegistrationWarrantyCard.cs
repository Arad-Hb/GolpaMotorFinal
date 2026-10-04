using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainModel.Migrations
{
    /// <inheritdoc />
    public partial class UniqueCardRegistrationWarrantyCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE L
                FROM ReportActivityLogs AS L
                INNER JOIN (
                    SELECT CardRegisterationID,
                           ROW_NUMBER() OVER (PARTITION BY WarrantyCardID ORDER BY CardRegisterationID) AS rn
                    FROM CardRegistrations
                ) AS d ON L.CardRegistrationID = d.CardRegisterationID
                WHERE d.rn > 1;

                DELETE C
                FROM CardRegistrations AS C
                INNER JOIN (
                    SELECT CardRegisterationID,
                           ROW_NUMBER() OVER (PARTITION BY WarrantyCardID ORDER BY CardRegisterationID) AS rn
                    FROM CardRegistrations
                ) AS d ON C.CardRegisterationID = d.CardRegisterationID
                WHERE d.rn > 1;
                """);

            migrationBuilder.DropIndex(
                name: "IX_CardRegistrations_WarrantyCardID",
                table: "CardRegistrations");

            migrationBuilder.CreateIndex(
                name: "IX_CardRegistrations_WarrantyCardID",
                table: "CardRegistrations",
                column: "WarrantyCardID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CardRegistrations_WarrantyCardID",
                table: "CardRegistrations");

            migrationBuilder.CreateIndex(
                name: "IX_CardRegistrations_WarrantyCardID",
                table: "CardRegistrations",
                column: "WarrantyCardID");
        }
    }
}
