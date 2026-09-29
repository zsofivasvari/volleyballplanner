using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolleyPlanner.API.Migrations
{
    /// <inheritdoc />
    public partial class LinkTrainingSessionsToCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_TrainingPlans_TrainingPlanId",
                table: "CalendarEvents");

            migrationBuilder.DropIndex(
                name: "IX_CalendarEvents_UserId",
                table: "CalendarEvents");

            migrationBuilder.AlterColumn<int>(
                name: "TrainingPlanId",
                table: "CalendarEvents",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TrainingSessionId",
                table: "CalendarEvents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_TrainingSessionId",
                table: "CalendarEvents",
                column: "TrainingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_UserId_TrainingSessionId",
                table: "CalendarEvents",
                columns: new[] { "UserId", "TrainingSessionId" },
                unique: true,
                filter: "[TrainingSessionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_TrainingPlans_TrainingPlanId",
                table: "CalendarEvents",
                column: "TrainingPlanId",
                principalTable: "TrainingPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_TrainingSessions_TrainingSessionId",
                table: "CalendarEvents",
                column: "TrainingSessionId",
                principalTable: "TrainingSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_TrainingPlans_TrainingPlanId",
                table: "CalendarEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_TrainingSessions_TrainingSessionId",
                table: "CalendarEvents");

            migrationBuilder.DropIndex(
                name: "IX_CalendarEvents_TrainingSessionId",
                table: "CalendarEvents");

            migrationBuilder.DropIndex(
                name: "IX_CalendarEvents_UserId_TrainingSessionId",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "TrainingSessionId",
                table: "CalendarEvents");

            migrationBuilder.AlterColumn<int>(
                name: "TrainingPlanId",
                table: "CalendarEvents",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_UserId",
                table: "CalendarEvents",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_TrainingPlans_TrainingPlanId",
                table: "CalendarEvents",
                column: "TrainingPlanId",
                principalTable: "TrainingPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
