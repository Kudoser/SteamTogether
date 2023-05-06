using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamTogether.Core.Migrations
{
    /// <inheritdoc />
    public partial class Categories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_SteamGameSteamCategory_CategoryId",
                table: "SteamGameSteamCategory",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SteamGameSteamCategory");

            migrationBuilder.DropTable(
                name: "SteamGamesCategories");
        }
    }
}
