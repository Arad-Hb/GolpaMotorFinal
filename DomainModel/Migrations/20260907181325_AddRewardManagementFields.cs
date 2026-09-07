using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainModel.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedDate",
                table: "RewardRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RewardDeliveryStatusID",
                table: "RewardRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RewardCatalogs",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasReceivedReward",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEligibleForReward",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_RewardRequests_RewardDeliveryStatusID",
                table: "RewardRequests",
                column: "RewardDeliveryStatusID");

            migrationBuilder.AddForeignKey(
                name: "FK_RewardRequests_RewardDeliveryStatuses_RewardDeliveryStatusID",
                table: "RewardRequests",
                column: "RewardDeliveryStatusID",
                principalTable: "RewardDeliveryStatuses",
                principalColumn: "RewardDeliveryStatusID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RewardRequests_RewardDeliveryStatuses_RewardDeliveryStatusID",
                table: "RewardRequests");

            migrationBuilder.DropIndex(
                name: "IX_RewardRequests_RewardDeliveryStatusID",
                table: "RewardRequests");

            migrationBuilder.DropColumn(
                name: "ReviewedDate",
                table: "RewardRequests");

            migrationBuilder.DropColumn(
                name: "RewardDeliveryStatusID",
                table: "RewardRequests");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RewardCatalogs");

            migrationBuilder.DropColumn(
                name: "HasReceivedReward",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsEligibleForReward",
                table: "AspNetUsers");
        }
    }
}
