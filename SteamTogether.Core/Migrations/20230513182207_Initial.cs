using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamTogether.Core.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SteamGames",
                columns: table => new
                {
                    GameId = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SteamAppId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    LastSyncDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamGames", x => x.GameId);
                });

            migrationBuilder.CreateTable(
                name: "SteamGamesCategories",
                columns: table => new
                {
                    CategoryId = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamGamesCategories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "SteamPlayers",
                columns: table => new
                {
                    PlayerId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    ApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    LastSyncDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamPlayers", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "SteamGameSteamCategory",
                columns: table => new
                {
                    GameId = table.Column<uint>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamGameSteamCategory", x => new { x.GameId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_SteamGameSteamCategory_SteamGamesCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "SteamGamesCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SteamGameSteamCategory_SteamGames_GameId",
                        column: x => x.GameId,
                        principalTable: "SteamGames",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "TelegramChatParticipants",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "INTEGER", nullable: false),
                    SteamPlayerId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    TelegramUserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramChatParticipants", x => new { x.ChatId, x.TelegramUserId, x.SteamPlayerId });
                    table.ForeignKey(
                        name: "FK_TelegramChatParticipants_SteamPlayers_SteamPlayerId",
                        column: x => x.SteamPlayerId,
                        principalTable: "SteamPlayers",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SteamGameSteamCategory_CategoryId",
                table: "SteamGameSteamCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SteamPlayerSteamGame_GameId",
                table: "SteamPlayerSteamGame",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramChatParticipants_ChatId_SteamPlayerId",
                table: "TelegramChatParticipants",
                columns: new[] { "ChatId", "SteamPlayerId" });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramChatParticipants_ChatId_TelegramUserId",
                table: "TelegramChatParticipants",
                columns: new[] { "ChatId", "TelegramUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramChatParticipants_SteamPlayerId",
                table: "TelegramChatParticipants",
                column: "SteamPlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SteamGameSteamCategory");

            migrationBuilder.DropTable(
                name: "SteamPlayerSteamGame");

            migrationBuilder.DropTable(
                name: "TelegramChatParticipants");

            migrationBuilder.DropTable(
                name: "SteamGamesCategories");

            migrationBuilder.DropTable(
                name: "SteamGames");

            migrationBuilder.DropTable(
                name: "SteamPlayers");
        }
    }
}
