using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamTogether.Bot.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SteamPlayers",
                columns: table => new
                {
                    PlayerId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApiKey = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamPlayers", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "TelegramChat",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramChat", x => x.ChatId);
                });

            migrationBuilder.CreateTable(
                name: "SteamPlayerTelegramChat",
                columns: table => new
                {
                    PlayerId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ChatId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamPlayerTelegramChat", x => new { x.PlayerId, x.ChatId });
                    table.ForeignKey(
                        name: "FK_SteamPlayerTelegramChat_SteamPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "SteamPlayers",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SteamPlayerTelegramChat_TelegramChat_ChatId",
                        column: x => x.ChatId,
                        principalTable: "TelegramChat",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SteamPlayerTelegramChat_ChatId",
                table: "SteamPlayerTelegramChat",
                column: "ChatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SteamPlayerTelegramChat");

            migrationBuilder.DropTable(
                name: "SteamPlayers");

            migrationBuilder.DropTable(
                name: "TelegramChat");
        }
    }
}
