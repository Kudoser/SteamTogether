using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamTogether.Core.Migrations
{
    /// <inheritdoc />
    public partial class Games : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SteamPlayers",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.CreateTable(
                name: "SteamGames",
                columns: table =>
                    new
                    {
                        GameId = table
                            .Column<uint>(type: "INTEGER", nullable: false)
                            .Annotation("Sqlite:Autoincrement", true),
                        SteamAppId = table.Column<uint>(type: "INTEGER", nullable: false),
                        Name = table.Column<string>(type: "TEXT", nullable: false),
                        Multiplayer = table.Column<bool>(type: "INTEGER", nullable: false),
                        LastSyncDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamGames", x => x.GameId);
                }
            );

            migrationBuilder.CreateTable(
                name: "SteamPlayerSteamGame",
                columns: table =>
                    new
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
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_SteamPlayerSteamGame_SteamPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "SteamPlayers",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_SteamPlayerSteamGame_GameId",
                table: "SteamPlayerSteamGame",
                column: "GameId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SteamPlayerSteamGame");

            migrationBuilder.DropTable(name: "SteamGames");

            migrationBuilder.DropColumn(name: "Name", table: "SteamPlayers");
        }
    }
}
