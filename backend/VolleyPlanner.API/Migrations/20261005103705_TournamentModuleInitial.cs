using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolleyPlanner.API.Migrations
{
    /// <inheritdoc />
    public partial class TournamentModuleInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlayerCode",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Player1UserId = table.Column<int>(type: "int", nullable: false),
                    Player2UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.CheckConstraint("CK_Team_DifferentPlayers", "[Player1UserId] <> [Player2UserId]");
                    table.ForeignKey(
                        name: "FK_Teams_Users_Player1UserId",
                        column: x => x.Player1UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Teams_Users_Player2UserId",
                        column: x => x.Player2UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tournaments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizerUserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegistrationDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxTeams = table.Column<int>(type: "int", nullable: false),
                    Format = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tournaments_Users_OrganizerUserId",
                        column: x => x.OrganizerUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeamInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    InviterUserId = table.Column<int>(type: "int", nullable: false),
                    InvitedUserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamInvitations_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamInvitations_Users_InvitedUserId",
                        column: x => x.InvitedUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamInvitations_Users_InviterUserId",
                        column: x => x.InviterUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TournamentEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentEntries_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TournamentEntries_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentPools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PoolNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentPools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentPools_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    TournamentPoolId = table.Column<int>(type: "int", nullable: true),
                    MatchNumber = table.Column<int>(type: "int", nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Team1EntryId = table.Column<int>(type: "int", nullable: true),
                    Team2EntryId = table.Column<int>(type: "int", nullable: true),
                    Team1SourceMatchId = table.Column<int>(type: "int", nullable: true),
                    Team2SourceMatchId = table.Column<int>(type: "int", nullable: true),
                    Team1SourceType = table.Column<int>(type: "int", nullable: false),
                    Team2SourceType = table.Column<int>(type: "int", nullable: false),
                    Team1SetsWon = table.Column<int>(type: "int", nullable: true),
                    Team2SetsWon = table.Column<int>(type: "int", nullable: true),
                    WinnerEntryId = table.Column<int>(type: "int", nullable: true),
                    LoserEntryId = table.Column<int>(type: "int", nullable: true),
                    IsAutomaticResult = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentMatches_TournamentEntries_LoserEntryId",
                        column: x => x.LoserEntryId,
                        principalTable: "TournamentEntries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TournamentMatches_TournamentEntries_Team1EntryId",
                        column: x => x.Team1EntryId,
                        principalTable: "TournamentEntries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TournamentMatches_TournamentEntries_Team2EntryId",
                        column: x => x.Team2EntryId,
                        principalTable: "TournamentEntries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TournamentMatches_TournamentEntries_WinnerEntryId",
                        column: x => x.WinnerEntryId,
                        principalTable: "TournamentEntries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TournamentMatches_TournamentMatches_Team1SourceMatchId",
                        column: x => x.Team1SourceMatchId,
                        principalTable: "TournamentMatches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TournamentMatches_TournamentMatches_Team2SourceMatchId",
                        column: x => x.Team2SourceMatchId,
                        principalTable: "TournamentMatches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TournamentMatches_TournamentPools_TournamentPoolId",
                        column: x => x.TournamentPoolId,
                        principalTable: "TournamentPools",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TournamentMatches_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentPoolSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentPoolId = table.Column<int>(type: "int", nullable: false),
                    TournamentEntryId = table.Column<int>(type: "int", nullable: true),
                    SlotNumber = table.Column<int>(type: "int", nullable: false),
                    FinalRank = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentPoolSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentPoolSlots_TournamentEntries_TournamentEntryId",
                        column: x => x.TournamentEntryId,
                        principalTable: "TournamentEntries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TournamentPoolSlots_TournamentPools_TournamentPoolId",
                        column: x => x.TournamentPoolId,
                        principalTable: "TournamentPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentMatchSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentMatchId = table.Column<int>(type: "int", nullable: false),
                    SetNumber = table.Column<int>(type: "int", nullable: false),
                    Team1Points = table.Column<int>(type: "int", nullable: false),
                    Team2Points = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentMatchSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentMatchSets_TournamentMatches_TournamentMatchId",
                        column: x => x.TournamentMatchId,
                        principalTable: "TournamentMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PlayerCode",
                table: "Users",
                column: "PlayerCode",
                unique: true,
                filter: "[PlayerCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvitations_InvitedUserId",
                table: "TeamInvitations",
                column: "InvitedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvitations_InviterUserId",
                table: "TeamInvitations",
                column: "InviterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvitations_TournamentId",
                table: "TeamInvitations",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Player1UserId",
                table: "Teams",
                column: "Player1UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Player2UserId",
                table: "Teams",
                column: "Player2UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentEntries_TeamId",
                table: "TournamentEntries",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentEntries_TournamentId_TeamId",
                table: "TournamentEntries",
                columns: new[] { "TournamentId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_LoserEntryId",
                table: "TournamentMatches",
                column: "LoserEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_Team1EntryId",
                table: "TournamentMatches",
                column: "Team1EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_Team1SourceMatchId",
                table: "TournamentMatches",
                column: "Team1SourceMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_Team2EntryId",
                table: "TournamentMatches",
                column: "Team2EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_Team2SourceMatchId",
                table: "TournamentMatches",
                column: "Team2SourceMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_TournamentId_MatchNumber",
                table: "TournamentMatches",
                columns: new[] { "TournamentId", "MatchNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_TournamentPoolId",
                table: "TournamentMatches",
                column: "TournamentPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_WinnerEntryId",
                table: "TournamentMatches",
                column: "WinnerEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatchSets_TournamentMatchId_SetNumber",
                table: "TournamentMatchSets",
                columns: new[] { "TournamentMatchId", "SetNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPools_TournamentId_PoolNumber",
                table: "TournamentPools",
                columns: new[] { "TournamentId", "PoolNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPoolSlots_TournamentEntryId",
                table: "TournamentPoolSlots",
                column: "TournamentEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPoolSlots_TournamentPoolId_SlotNumber",
                table: "TournamentPoolSlots",
                columns: new[] { "TournamentPoolId", "SlotNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_OrganizerUserId",
                table: "Tournaments",
                column: "OrganizerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamInvitations");

            migrationBuilder.DropTable(
                name: "TournamentMatchSets");

            migrationBuilder.DropTable(
                name: "TournamentPoolSlots");

            migrationBuilder.DropTable(
                name: "TournamentMatches");

            migrationBuilder.DropTable(
                name: "TournamentEntries");

            migrationBuilder.DropTable(
                name: "TournamentPools");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Users_PlayerCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PlayerCode",
                table: "Users");
        }
    }
}
