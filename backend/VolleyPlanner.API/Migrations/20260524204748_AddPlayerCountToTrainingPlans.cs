using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolleyPlanner.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerCountToTrainingPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerCount",
                table: "TrainingPlans",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerCount",
                table: "TrainingPlans");
        }
    }
}
