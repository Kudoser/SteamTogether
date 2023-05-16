using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamTogether.Core.Migrations
{
    /// <inheritdoc />
    public partial class TimePlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SteamPlayerSteamGame");

            migrationBuilder.CreateTable(
                name: "PlayerGame",
                columns: table => new
                {
                    GameId = table.Column<uint>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    PlaytimeForever = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    PlaytimeLastTwoWeeks = table.Column<TimeSpan>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerGame", x => new { x.GameId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_PlayerGame_SteamGames_GameId",
                        column: x => x.GameId,
                        principalTable: "SteamGames",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGame_SteamPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "SteamPlayers",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGame_PlayerId",
                table: "PlayerGame",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerGame");

            migrationBuilder.CreateTable(
                name: "SteamPlayerSteamGame",
                columns: table => new
                {
                    PlayerId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    GameId = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamPlayerSteamGame", x => new { x.PlayerId, x.GameId });
                    table.ForeignKey(
                        name: "FK_SteamPlayerSteamGame_SteamGames_GameId",
                        column: x => x.GameId,
                        principalTable: "SteamGames",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SteamPlayerSteamGame_SteamPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "SteamPlayers",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SteamPlayerSteamGame_GameId",
                table: "SteamPlayerSteamGame",
                column: "GameId");
        }
    }
}
