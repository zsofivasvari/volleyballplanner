using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolleyPlanner.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingPlanModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GenerationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SportType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationMin = table.Column<int>(type: "int", nullable: false),
                    PlayerCount = table.Column<int>(type: "int", nullable: false),
                    Intensity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryFocus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableEquipment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GenerationRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SportType = table.Column<int>(type: "int", nullable: false),
                    PlanType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetDuration = table.Column<int>(type: "int", nullable: false),
                    TargetIntensity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryFocus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingPlans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingPlanItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainingPlanId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    SectionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlannedDuration = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingPlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingPlanItems_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainingPlanItems_TrainingPlans_TrainingPlanId",
                        column: x => x.TrainingPlanId,
                        principalTable: "TrainingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GenerationRequests_UserId",
                table: "GenerationRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlanItems_ExerciseId",
                table: "TrainingPlanItems",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlanItems_TrainingPlanId",
                table: "TrainingPlanItems",
                column: "TrainingPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_UserId",
                table: "TrainingPlans",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenerationRequests");

            migrationBuilder.DropTable(
                name: "TrainingPlanItems");

            migrationBuilder.DropTable(
                name: "TrainingPlans");
        }
    }
}
