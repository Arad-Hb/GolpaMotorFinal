using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainModel.Migrations
{
    /// <inheritdoc />
    public partial class UnlockAvailableUntilApprove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE ReportActivityLogs
                SET AvailablePoints = RemainedPoints
                WHERE AvailablePoints <> RemainedPoints;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
