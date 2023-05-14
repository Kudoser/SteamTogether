using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamTogether.Core.Migrations
{
    /// <inheritdoc />
    public partial class Polls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SteamGamesCategories",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SteamGames",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "TelegramPolls",
                columns: table => new
                {
                    PollId = table.Column<string>(type: "TEXT", nullable: false),
                    ChatId = table.Column<long>(type: "INTEGER", nullable: false),
                    MessageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramPolls", x => x.PollId);
                });

            migrationBuilder.CreateTable(
                name: "TelegramPollVotes",
                columns: table => new
                {
                    PollVoteId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PollId = table.Column<string>(type: "TEXT", nullable: false),
                    TelegramUserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramPollVotes", x => x.PollVoteId);
                    table.ForeignKey(
                        name: "FK_TelegramPollVotes_TelegramPolls_PollId",
                        column: x => x.PollId,
                        principalTable: "TelegramPolls",
                        principalColumn: "PollId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramPolls_ChatId",
                table: "TelegramPolls",
                column: "ChatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramPollVotes_PollId",
                table: "TelegramPollVotes",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramPollVotes_PollId_TelegramUserId",
                table: "TelegramPollVotes",
                columns: new[] { "PollId", "TelegramUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelegramPollVotes");

            migrationBuilder.DropTable(
                name: "TelegramPolls");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SteamGamesCategories",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SteamGames",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
