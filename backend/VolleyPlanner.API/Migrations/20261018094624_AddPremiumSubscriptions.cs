using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolleyPlanner.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPremiumSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FreeGenerationsUsed",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderCustomerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProviderSubscriptionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CurrentPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelAtPeriodEnd = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_ProviderCustomerId",
                table: "UserSubscriptions",
                column: "ProviderCustomerId",
                unique: true,
                filter: "[ProviderCustomerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_ProviderSubscriptionId",
                table: "UserSubscriptions",
                column: "ProviderSubscriptionId",
                unique: true,
                filter: "[ProviderSubscriptionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "FreeGenerationsUsed",
                table: "Users");
        }
    }
}
