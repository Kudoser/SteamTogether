using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamTogether.Bot.Migrations
{
    /// <inheritdoc />
    public partial class LastSyncDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncDateTime",
                table: "SteamPlayers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSyncDateTime",
                table: "SteamPlayers");
        }
    }
}
